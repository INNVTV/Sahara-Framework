(function () {
    'use strict';

    var serviceId = 'accountListServices';
    var urlBase = '/Accounts/Json';

    //Used for caching search page:
    var cacheExists = false;
    var cache_accounts = [];
    var cache_currentPage
    var cache_numPages
    var cache_totalAccounts
    var cache_mode
    var cache_currentFilter
    var cache_currentSortBy
    var cache_currentSortDirection
    var cache_currentAmountPerPage

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', accountListServices]);

    function accountListServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            //get object count for pagination purposes
            getCount: getCount,
            getCountByFilter: getCountByFilter,

            //get account lists
            getAccounts: getAccounts,
            getAccountsByFilter: getAccountsByFilter,

            //search Accounts
            searchAccounts: searchAccounts,




            //Methods for storing/retreiving cache data
            //store/get list state: (used for navigating back to list from detail pages (services do not lose state between controllers/views
            storeAccountListState   :   storeAccountListState,
            storeCurrentMode: storeCurrentMode,

            getCacheStatus : getCacheStatus,
            getAccountListState : getAccountListState,
            getCurrentPageState : getCurrentPageState,
            getNumPagesState: getNumPagesState,
            getTotalAccountsState : getTotalAccountsState,
            getModeState : getModeState,
            getFilterState : getFilterState,
            getSortByState : getSortByState,
            getSortDirectionState : getSortDirectionState,
            getAmountPerPageState : getAmountPerPageState,


        };

        return service;

        // Get count of ALL accounts available
        function getCount() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/Count' + cacheBuster });
        }

        // Get count of ALL accounts available
        function getCountByFilter(filter, value) {

            var date = new Date();
            var cacheBuster = "&cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/Count/ByFilter?filter=' + filter + '&value=' + value + cacheBuster });
        }





        // Get all accounts available
        //function getAccounts(page, amountPerPage) {
            //return $http({ method: 'GET', url: urlBase + '/Get/' + page + '/' + amountPerPage + '/CreatedDate/Desc' });
        //}
        // Get all accounts available
        function getAccounts(page, amountPerPage, sortBy, orderBy) {

            var date = new Date();
            var cacheBuster = "&cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/Get?page=' + page + '&amount=' + amountPerPage + '&sortBy=' + sortBy + '&direction=' + orderBy + cacheBuster });
        }

        // Get filtered accounts available
        function getAccountsByFilter(filter, value, pageNumber, amountPerPage, sortBy, orderBy) {

            var date = new Date();
            var cacheBuster = "&cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/Get/ByFilter?page=' + pageNumber + '&amount=' + amountPerPage + '&filter=' + filter + '&value=' + value + '&sortBy=' + sortBy + '&direction=' + orderBy + cacheBuster });
        }



        // Serch accounts
        function searchAccounts(query, maxResults) {

            var date = new Date();
            var cacheBuster = "&cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/Search?query=' + query + '&maxResults=' + maxResults + cacheBuster});
        }

        /*
        function storeAccountListState(accountsIn)
        {
            accounts = accountsIn;
        }
        function getAccountListState()
        {
            return accounts;
        }*/



        //Caching Methods:

        function storeAccountListState(accountsIn, currentPageIn, numPagesIn, totalAccountsIn, modeIn, currentFilterIn, currentSortByIn, currentSortDirectionIn, currentAmountPerPageIn) {
            
            cacheExists = true;

            cache_accounts = accountsIn;

            cache_currentPage = currentPageIn;
            cache_numPages = numPagesIn;
            cache_totalAccounts = totalAccountsIn;
            cache_mode = modeIn;
            cache_currentFilter = currentFilterIn;
            cache_currentSortBy = currentSortByIn;
            cache_currentSortDirection = currentSortDirectionIn;
            cache_currentAmountPerPage = currentAmountPerPageIn;
        }

        function storeCurrentMode(mode)
        {
            cache_mode = mode;
        }

        function getCacheStatus() {
            return cacheExists;
        }

        function getAccountListState() {
            return cache_accounts;
        }


        function getCurrentPageState() {
            return cache_currentPage;
        }

        function getNumPagesState() {
            return cache_numPages;
        }

        function getTotalAccountsState() {
            return cache_totalAccounts;
        }

        function getModeState() {
            return cache_mode;
        }

        function getFilterState() {
            return cache_currentFilter;
        }

        function getSortByState() {
            return cache_currentSortBy;
        }

        function getSortDirectionState() {
            return cache_currentSortDirection;
        }

        function getAmountPerPageState() {
            return cache_currentAmountPerPage;
        }

        // Get all accounts available
        //function getAccountDetails() {
            //return $http({ method: 'GET', url: urlBase + '/Count' });
        //}

        //#region Internal Methods        

        //#endregion
    }
})();