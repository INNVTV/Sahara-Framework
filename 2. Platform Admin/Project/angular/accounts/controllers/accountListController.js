(function () {
    'use strict';

    var controllerId = 'accountListController';

    // Inject into main app module:
    angular.module('app')
        .controller(controllerId, [
            'accountListServices',
            'accountListModels',
            'plansServices',
            accountListController
        ]);

    function accountListController(accountListServices, accountListModels, plansServices) {

        //Instantiate Controller as ViewModel
        var vm = this;

        //Default Properties: =============================
        vm.title = 'accountListController';
        vm.activate = activate;

        // Debug Methods ===================================
        // wrap console.log() within the "Debug" namespace
        //    -->  Logs can be removed during minification
        var Debug = {};
        Debug.trace = function (message) {
            console.log(message);
        };



        // Controller Properties

        vm.cacheStatus = false;

        vm.accounts = [];

        vm.browseMode = true;
        vm.searchMode = false;

        vm.loadMore = false; //<--- flag used if "Load more" is used on mobile, will append rather than refresh account list
        vm.showLoadMoreButton = false;

        vm.showPagination = false;
        vm.totalAccounts = 0;
        vm.currentPage = 1;
        vm.maxPaginationLinksVisible = 10;
        vm.amountOnPage = 0;
        vm.searchResultsCount = 0;
        vm.maxSearchResults = 250;


        vm.showGetAccountsLoading = true;
        vm.showAccountsListLoaded = false;

        vm.currentFilter = null
        vm.currentSortBy = null;
        vm.currentAmountPerPage = null;

        //vm.currentPageState = null;

        vm.showFilterResults = false;
        vm.showSearchResults = false;

        /*
        <ng-pluralize count="vm.totalAccounts" when="vm.accountCount" /></ng-pluralize>
        vm.accountCount = {
            0: "No accounts found",
            other: "{} total accounts (showing x - x)"
        }
        


        vm.pageChanged= function()
        {
            vm.getFilteredAccounts(vm.currentFilterLabel, vm.currentFilterValue, vm.currentPage, vm.currentSortBy, vm.currentOrderBy)
        }*/

                        


        // Browse Methods ====================================================
        //====================================================================

        //Update Methods for DropDowns:

        vm.updateCurrentFilter = function(filter) {
            vm.currentFilter = filter;
            ///#DEBUG
            //Alternative way to denote "debug only"  areas
            console.log('Updated filter to: ' + vm.currentFilter.label);
            ///#ENDDEBUG
        }
        vm.updateCurrentSortBy = function(sortBy) {
            vm.currentSortBy = sortBy
            Debug.trace('Updated sortBy to: ' + vm.currentSortBy.label);
        }
        vm.updateCurrentSortDirection = function(direction) {
            vm.currentSortDirection = direction
            Debug.trace('Updated sortDirection to: ' + vm.currentSortDirection.label);
        }
        vm.updateCurrentAmountPerPage = function(amountPerPage) {
            vm.currentAmountPerPage = amountPerPage
            Debug.trace('Updated amountPerPage to: ' + vm.currentAmountPerPage.label);
        }

        //Get Filtered Account Results: Used on "Update Results" Button Click to reset pagination page to "1"
        vm.updateResults = function () {
            vm.currentPage = 1;
            vm.getAccounts();
        }
        //Get Filtered Account Results: Used on Controller Activation & Paginating through result sets
        vm.getAccounts = function () {

            vm.searchMode = false;
            vm.browseMode = true;

            if(vm.currentFilter.value == "*")
            {

                Debug.trace('Getting ALL accounts');
                
                if (vm.loadMore == true)
                {
                    vm.showLoadMoreButton = false;
                    vm.showLoadMoreLoading = true;
                    
                }
                else {
                    vm.showGetAccountsLoading = true;
                    vm.showAccountsListLoaded = false;
                }



                // Get count of Account objects for pagination
                accountListServices.getCount()
                    .success(function (data, status, headers, config) {
                        Debug.trace("Getting count of total accounts (all)...");
                        vm.totalAccounts = parseInt(data);
                        Debug.trace("Total available:" + vm.totalAccounts);
                        vm.updatePagination();

                        // Get Initial Accounts List
                        //accountListServices.getAccounts(vm.currentPage, vm.currentAmountPerPage.value)
                        accountListServices.getAccounts(vm.currentPage, vm.currentAmountPerPage.value, vm.currentSortBy.value, vm.currentSortDirection.value)
                            .success(function (data, status, headers, config) {

                                //Update UTC to local time on the client side:
                                data.forEach(function (account) {
                                    //account.CreatedDate = new Date(account.CreatedDate).toLocaleString();
                                    account.CreatedDate = new Date(account.CreatedDate.toLocaleString());

                                });

                                if (vm.loadMore == true)
                                {
                                    vm.accounts = vm.accounts.concat(data);
                                    vm.loadMore = false;

                                    vm.showLoadMoreLoading = false;

                                    //Check if button should stay visible (more pages available):
                                    //if (vm.currentPage == vm.numPages) {
                                    //   vm.showLoadMoreButton = false;
                                    //}
                                    //else {
                                    //   vm.showLoadMoreButton = true;
                                    //}
                                }
                                else
                                {
                                    vm.accounts = data;
                                }


                                vm.amountOnPage = data.length;
                                vm.showGetAccountsLoading = false;
                                vm.showAccountsListLoaded = true;
                                vm.updatePagination();
                                vm.displayFilterResults();

                                //store current state into the cache:
                                var currentMode = vm.getCurrentMode()
                                Debug.trace("currentMode:" + currentMode);
                                accountListServices.storeAccountListState(vm.accounts, vm.currentPage, vm.numPages, vm.totalAccounts, currentMode, vm.currentFilter, vm.currentSortBy, vm.currentSortDirection, vm.currentAmountPerPage);

                            })
                            .error(function (data, status, headers, config) {
                                //
                            })


                    })
                    .error(function (data, status, headers, config) {
                        vm.totalAccounts = 0;
                    })





            }
            else
            {
                Debug.trace('Getting filtered accounts');

                //vm.showGetAccountsLoading = true;
                //vm.showAccountsListLoaded = false;

                if (vm.loadMore == true) {
                    vm.showLoadMoreButton = false;
                    vm.showLoadMoreLoading = true;
                }
                else {
                    vm.showGetAccountsLoading = true;
                    vm.showAccountsListLoaded = false;
                }


                // Get count of Account objects for pagination
                accountListServices.getCountByFilter(vm.currentFilter.name, vm.currentFilter.value)
                    .success(function (data, status, headers, config) {

                        Debug.trace("Getting count of total accounts (filtered)...");

                        vm.totalAccounts = parseInt(data);

                        Debug.trace("Total available:" + vm.totalAccounts);

                        vm.updatePagination();
                        vm.displayFilterResults();


                        accountListServices.getAccountsByFilter(vm.currentFilter.name, vm.currentFilter.value, vm.currentPage, vm.currentAmountPerPage.value, vm.currentSortBy.value, vm.currentSortDirection.value)
                            .success(function (data, status, headers, config) {
                        

                        //Update UTC to local time on the client side:
                        data.forEach(function (account) {
                            //account.CreatedDate = new Date(account.CreatedDate).toLocaleString();
                            account.CreatedDate = new Date(account.CreatedDate.toLocaleString());
                        });


                        if (vm.loadMore == true) {
                            vm.accounts = vm.accounts.concat(data);
                            vm.loadMore = false;

                            vm.showLoadMoreLoading = false;

                        }
                        else {
                            vm.accounts = data;
                        }




                        //store accounts into the sates:
                        //accountListServices.storeAccountListState(vm.accounts);

                        vm.amountOnPage = data.length;
                        vm.showGetAccountsLoading = false;
                        vm.showAccountsListLoaded = true;
                        vm.updatePagination();
                        //store current state into the cache:
                        var currentMode = vm.getCurrentMode()
                        Debug.trace("currentMode:" + currentMode);
                        accountListServices.storeAccountListState(vm.accounts, vm.currentPage, vm.numPages, vm.totalAccounts, currentMode, vm.currentFilter, vm.currentSortBy, vm.currentSortDirection, vm.currentAmountPerPage);
                        
                    })
                    .error(function (data, status, headers, config) {
                        //
                    })

                    })
                    .error(function (data, status, headers, config) {
                        vm.totalAccounts = 0;
                    })


            }

        }



        // Search Methods ====================================================
        //===================================================================

        vm.searchAccounts = function () {

            //vm.searchMode = true;
            //vm.browseMode = false;
            vm.setCurrentMode("search");

            //Load more button will always be hidden after a search due to the limited result set:
            vm.showLoadMoreButton = false;

            Debug.trace('Searching accounts...');

            vm.showPagination = false;

            vm.showGetAccountsLoading = true;
            vm.showAccountsListLoaded = false;

            accountListServices.searchAccounts(vm.searchQuery, vm.maxSearchResults)
                .success(function (data, status, headers, config) {
                    

                    //Update UTC to local time on the client side:
                    data.forEach(function (account) {
                        //account.CreatedDate = new Date(account.CreatedDate).toLocaleString();
                        account.CreatedDate = new Date(account.CreatedDate.toLocaleString());
                    });

                    //assign to view model:
                    vm.accounts = data;

                    //store accounts into the sates:
                    //accountListServices.storeAccountListState(vm.accounts);

                    vm.searchResultsCount = data.length;
                    vm.showGetAccountsLoading = false;
                    vm.showAccountsListLoaded = true;


                    vm.displaySearchResults();

                    //store current state into the cache:
                    var currentMode = vm.getCurrentMode()
                    Debug.trace("currentMode:" + currentMode);
                    accountListServices.storeAccountListState(vm.accounts, vm.currentPage, vm.numPages, vm.totalAccounts, currentMode, vm.currentFilter, vm.currentSortBy, vm.currentSortDirection, vm.currentAmountPerPage);

                })
                .error(function (data, status, headers, config) {
                    //
                })
        }


        // (Mobile) Load More... Methods ====================================================
        //===================================================================

        vm.loadMoreOnMobile = function () {
            vm.loadMore = true;
            //vm.showLoadMoreButton = false;

            Debug.trace('Loading more on mobile view...');
            
            //Update pagination
            vm.currentPage = vm.currentPage + 1;

            
            vm.getAccounts();

            //Check if button should stay visible (more pages available):
            if (vm.currentPage == vm.numPages) {
                vm.showLoadMoreButton = false;
            }
            else
            {
                vm.showLoadMoreButton = true;
            }
        }



        // Toggle ====================================================
        //===================================================================

        vm.toggleMode = function () {

            Debug.trace("Toggle mode called, searchMode=" + vm.searchMode + " | browseMode =" + vm.browseMode);

            if(vm.searchMode)
            {
                vm.searchMode = false;
                vm.browseMode = true;
            }
            else
            {
                vm.searchMode = true;
                vm.browseMode = false;

            }

            //Update the cache with the current mode:
            var currentMode = vm.getCurrentMode();
            accountListServices.storeCurrentMode(currentMode);


            //Debug.trace("Toggle mode complete, searchMode=" + vm.searchMode + " | browseMode =" + vm.browseMode);

           
        }


        // Internal Page State Update Methods ====================================================
        //====================================================================

        vm.updatePagination = function () {

            Debug.trace("Updating pagination...");
            Debug.trace("Current page:" + vm.currentPage + " Total pages:" + vm.numPages);

            var currentMode = vm.getCurrentMode();
            if (currentMode == "search")
            {
                vm.showPagination = false;
            }
            else if (vm.totalAccounts > vm.currentAmountPerPage.value) {
                vm.showPagination = true;
            }
            else {
                vm.showPagination = false;
            }

            //Check if loadMore button should stay/be visible on mobile (if more pages available):
            if (currentMode == "search") {
                vm.showLoadMoreButton = false;
            }
            else if (vm.currentPage == vm.numPages) {
                vm.showLoadMoreButton = false;
            }
            else {
                vm.showLoadMoreButton = true;
            }
        }

        vm.displayFilterResults = function () {
            vm.showFilterResults = true;
            vm.showSearchResults = false;
        }

        vm.displaySearchResults = function () {
            vm.showFilterResults = false;
            vm.showSearchResults = true;
        }


        vm.getCurrentMode = function () {

            Debug.trace("Getting current mode, searchMode=" + vm.searchMode + " | browseMode =" + vm.browseMode);

            if (vm.browseMode == true) {
                return "browse";
            }
            else {
                return "search";
            }
        }
        vm.setCurrentMode = function (mode) {
            
            Debug.trace("Mode is: " + mode);

            if (mode == "browse") {
                vm.browseMode = true;
                vm.searchMode = false;
                Debug.trace("Mode set to BROWSE");
            }
            else {
                vm.browseMode = false;
                vm.searchMode = true;
                Debug.trace("Mode set to SEARCH");
            }

            //Update the cache with the current mode:
            accountListServices.storeCurrentMode(mode);
        }


        // Controller Initialization  ========================================
        //=====================================================================

        activate();

        function activate() {

            //vm.currentPageState = accountListModels.getPageState();
            
            vm.filters = accountListModels.getFilters();
            vm.sortBy = accountListModels.getSortBy();
            vm.sortDirection = accountListModels.getSortDirection();
            vm.amountsPerPage = accountListModels.getAmountsPerPage();

            //Load up all availabble plans and inject into dropdown list filters
            plansServices.getPlans()
                            .success(function (data, status, headers, config) {

                                //Inject Into Filters
                                data.forEach(function (plan) {
                                    //account.CreatedDate = new Date(account.CreatedDate).toLocaleString();
                                    //account.CreatedDate = new Date(account.CreatedDate.toLocaleString());
                                    vm.filters.push({ label: plan.PaymentPlanName + " Plans", name: 'Accounts.PaymentPlanName', value: plan.PaymentPlanName })
                                });
                            })
                            .error(function (data, status, headers, config) {
                                //
                            })



            //vm.currentPageState.currentFilter = vm.filters[0];
            //vm.currentPageState.currentSortBy = vm.sortBy[0];
            //vm.currentPageState.currentSortDirection = vm.sortDirection[0];
            //vm.currentPageState.currentAmountPerPage = vm.amountsPerPage[0];

            vm.currentFilter = vm.filters[0];
            vm.currentSortBy = vm.sortBy[0];
            vm.currentSortDirection = vm.sortDirection[0];
            vm.currentAmountPerPage = vm.amountsPerPage[0];

            //check state, if no accounts are stored then call service to get accounts:
            vm.cacheStatus = accountListServices.getCacheStatus();


            if (vm.cacheStatus == false)
            {
                //First run, so get all accounts:
                Debug.trace("First run, get accounts called.");
                vm.getAccounts();
            }
            else
            {

                Debug.trace("Cache exists, initiating controller from cache");

                vm.accounts = accountListServices.getAccountListState();

                Debug.trace("Cache has " + vm.accounts.length + " accounts...");

                vm.currentPage = accountListServices.getCurrentPageState();
                vm.numPages = accountListServices.getNumPagesState();
                vm.totalAccounts = accountListServices.getTotalAccountsState();
                

                var currentMode = accountListServices.getModeState();
                vm.setCurrentMode(currentMode);

                vm.currentFilter = accountListServices.getFilterState();
                vm.currentSortBy = accountListServices.getSortByState();
                vm.currentSortDirection = accountListServices.getSortDirectionState();
                vm.currentAmountPerPage = accountListServices.getAmountPerPageState();

                //There is account data in the state, update UI and get other state info:
                vm.amountOnPage = vm.accounts.length;
                vm.showGetAccountsLoading = false;
                vm.showAccountsListLoaded = true;
                vm.updatePagination();

                vm.displayFilterResults();
            }

            Debug.trace('AccountController activation complete');
            
        }



        

    }

})();
