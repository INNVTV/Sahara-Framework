(function () {
    'use strict';

    var controllerId = 'inventoryIndexController';

    // Inject into main app module:
    angular.module('app')
        .controller(controllerId, [
            'inventoryIndexServices',
            'inventoryIndexModels',
            'sharedServices',
            'categoryServices',
            'accountServices',
            inventoryIndexController
    ]);

    function inventoryIndexController(inventoryIndexServices, inventoryIndexModels, sharedServices, categoryServices, accountServices) {

        //Instantiate Controller as ViewModel
        var vm = this;

        //Default Properties: =============================
        vm.title = 'inventoryIndexController';
        vm.activate = activate;

        /* ==========================================
               Helper Methods
        ==========================================*/

        // Debug Methods ===================================
        // wrap console.log() within the "Debug" namespace
        //    -->  Logs can be removed during minification
        var Debug = {};
        Debug.trace = function (message) {
            console.log(message);
        };


        //Account User:
        vm.currentUserProfile = null;
        var currentUserRoleIndex = null //<-- used internally to check role access, must be updated when getting or refreshing the user.
        var userRoles = []; //<-- used internally to check role access, must be updated when getting or refreshing the user.
        //--------------------------------------------------------------------

        /* ==========================================
             Core Service Properties
        ==========================================*/

        // vm.TrialHoldDays = null; //<----comes from CoreServices (via local feed)
        //vm.CustodianFrequencyDescription = null; //<----comes from CoreServices (via local feed)
        //vm.UnverifiedAccountsDaysToHold = null; //<----comes from CoreServices (via local feed)
        //vm.PlatformWorkerFrequencyDescription = null; //<----comes from CoreServices (via local feed)



        /* ==========================================
                Controller Properties
        ==========================================*/

        vm.showLoader = true;

        

        /* ==========================================
               Controller Models
       ==========================================*/

        vm.goToCategory = function (id) {
            window.location.href = 'category/' + id;
        }



        /* ==========================================
               CATEGORY METHODS
        ==========================================*/



        vm.categories = null;

        vm.getCategories = function() {
            //Debug.trace("Getting categories...");

            categoryServices.getCategories()
            .success(function (data, status, headers, config) {

                vm.categories = data;
                //vm.cancelOrdering();
                //Debug.trace(data);
                //Debug.trace(vm.categories);
                //Debug.trace(vm.categories[0]);

                //Debug.trace("Categories received!");

            })
                .error(function (data, status, headers, config) {


                })
        }

        vm.categoryConstraint = false; //<-- True if plan needs an upgrade

        vm.newCategory =
             {
                 Visible: true,
                 Name: null,

                 // Service Processing --------

                 IsSending: false,
                 SendingComplete: false,

                 Results: {
                     IsSuccess: false,
                     Message: null
                 },

                 // Visibiliy ----

                 Hide: function () {
                     this.Visible = false;
                 },

                 Show: function () {
                     this.Visible = true;
                 },

                 // Cleanup Routine(s) ----------

                 Retry: function () {
                     this.IsSending = false;
                     this.SendingComplete = false;
                 },

                 Clear: function () {
                     //Debug.trace("Clearing new category form data.");

                     this.Name = null;
                     this.Visible = true;

                     this.IsSending = false;
                     this.SendingComplete = false;

                     this.Results.IsSuccess = false;
                     this.Results.Message = null;
                 }
             }

        vm.newCategoryPath = null;
        vm.createCategory = function () {

            vm.categoryConstraint = false;
            vm.newCategory.IsSending = true;

            //Debug.trace("Creating category...");

            categoryServices.createCategory(vm.newCategory.Name, vm.newCategory.Visible)
            .success(function (data, status, headers, config) {

                vm.newCategory.IsSending = false;
                vm.newCategory.SendingComplete = true;

                if (data.isSuccess) {

                    vm.newCategory.Results.IsSuccess = true;
                    vm.newCategory.Results.Message = "Created!";
                    vm.newCategoryPath = data.SuccessMessage;
                    vm.getCategories();
                }
                else {
                    vm.newCategory.Results.IsSuccess = false;
                    vm.newCategory.Results.Message = data.ErrorMessage;

                    if (data.ErrorCode == "Constraint") {
                        vm.categoryConstraint = true;
                    }
                }

            })
                .error(function (data, status, headers, config) {

                    vm.newCategory.Results.IsSuccess = false;

                    //vm.clearInvitationForm();

                    vm.newCategory.IsSending = false;
                    vm.newCategory.SendingComplete = true;
                    vm.newCategory.Results.Message = "An error occurred while attempting to use the service...";
                })
        }

        vm.makeVisible = function (index) {
            vm.categories[index].Visible = null;

            categoryServices.updateCategoryVisibleState(vm.categories[index].CategoryID, true)
            .success(function (data, status, headers, config) {

                if (data.isSuccess) {
                    vm.categories[index].Visible = true;
                }
                else {
                    vm.categories[index].Visible = false;
                }
            })
                .error(function (data, status, headers, config) {
                    vm.categories[index].Visible = false;
                })
        }

        vm.makeHidden = function (index) {
            vm.categories[index].Visible = null;

            categoryServices.updateCategoryVisibleState(vm.categories[index].CategoryID, false)
            .success(function (data, status, headers, config) {

                if (data.isSuccess) {
                    vm.categories[index].Visible = false;
                }
                else {
                    vm.categories[index].Visible = true;
                }
            })
                .error(function (data, status, headers, config) {
                    vm.categories[index].Visible = true;
                })
        }

        /* ==========================================
            END CATEGORY METHODS
        ==========================================*/








        /* ==========================================

              GET ACCOUNT

        ==========================================*/


        vm.account = null

        vm.getAccount = function () {

            //Debug.trace('Getting account details...');

            accountServices.getAccount()
                    .success(function (data, status, headers, config) {
                        vm.account = data;
                        // Manage routes for subscribe / upgrade / card so the correct modal can be initiated
                        //Debug.trace("Route action: " + vm.routeAction);
                        
                    })
                    .error(function (data, status, headers, config) {
                        //
                    })
        }

        /* ==========================================
               END GET ACCOUNT
        ==========================================*/









        /* ==========================================
             LOG METHODS
        ==========================================*/

        vm.logs = null;

        vm.getLogs = function () {
            accountServices.getAccountLogByCategory("ApplicationCategorization", 50)
            .success(function (data, status, headers, config) {
                vm.logs = data;
            })
            .error(function (data, status, headers, config) {

            })
        }

        /* ==========================================
              END LOG METHODS
      ==========================================*/



        // Update Methods ---------------------------



        /* ==========================================
           CURRENT USER PROFILE
       ==========================================*/

        function updateCurrentUserProfile() {

            //Debug.trace("Refreshing user profile...");

            sharedServices.getCurrentUserProfile()
            .success(function (data, status, headers, config) {

                vm.currentUserProfile = data; //Used to determine what is shown in the view based on user Role.
                currentUserRoleIndex = vm.userRoles.indexOf(data.Role) //<-- use ACCOUNT roles, NOT PLATFORM roles!

                if (vm.currentUserProfile.Id == "" || vm.currentUserProfile == null)
                {
                    //Log user out if empty
                    window.location.replace("/login");
                }

                //Debug.trace("Profile refreshed!");
                //Debug.trace("Role index = " + currentUserRoleIndex);

            })
                .error(function (data, status, headers, config) {


                })

        }












        /* ==========================================

         REORDERING METHODS

       ==========================================*/


        vm.newOrder = null;
        vm.useAlphabeticalOrdering = false;
        vm.reorderProcessing = false;
        vm.reorderProcessingSuccess = false;
        vm.reorderProcessingFailed = false;
        vm.reorderErrorMessage = "";

        vm.startOrdering = function () {

            vm.reorderType = 'categories'

            vm.cancelOrdering();
            vm.newOrder = [];
 
            //vm.newOrder = vm.categories.slice(); //<-- Depricated

            //Pipe in values and rename to generic items----------------
            for (var i = 0, len = vm.categories.length; i < len; ++i)
            {
                var item = { ID: vm.categories[i].CategoryID, Name: vm.categories[i].CategoryName, Visible: vm.categories[i].Visible };
                vm.newOrder.push(item);
            }

 
        }

        vm.cancelOrdering = function () {
            vm.newOrder = null;
            vm.useAlphabeticalOrdering = false;
            vm.reorderProcessing = false;
            vm.reorderProcessingFailed = false;
            vm.reorderProcessingSuccess = false;
            vm.reorderErrorMessage = "";
        }

        vm.toggleAlphabeticalOrdering = function () {
            if(!vm.useAlphabeticalOrdering)
            {
                vm.useAlphabeticalOrdering = true;
            }
            else {
                vm.useAlphabeticalOrdering = false;
            }
            vm.newOrder.sort(function (a, b) {
                if (a.Name.toLowerCase() < b.Name.toLowerCase()) return -1;
                if (a.Name.toLowerCase() > b.Name.toLowerCase()) return 1;
                return 0;
            })

        }

        vm.saveOrder = function () {

            vm.reorderProcessing = true;

            if (vm.useAlphabeticalOrdering)
            {
                
                categoryServices.resetCategoryOrder()
                    .success(function (data, status, headers, config) {
                        vm.reorderProcessing = false;
                                        if (data.isSuccess) {
                                            vm.reorderProcessingSuccess = true;
                                            vm.getCategories();
                                        }
                                        else {
                                            vm.reorderProcessingFailed = true;
                                            vm.reorderProcessingSuccess = false;
                                            vm.reorderErrorMessage = data.ErrorMessage;
                                        }
                                    })
                    .error(function (data, status, headers, config) {
                        vm.reorderProcessing = false;
                                    vm.reorderProcessingFailed = true;
                                    vm.reorderProcessingSuccess = false;
                                    vm.reorderErrorMessage = "Could not connect!";
                                })
            }
            else
            {
                //Create array of IDs in the new order ----
                var idList = vm.newOrder.map(function (item) {
                    return item['ID'];
                });

                categoryServices.reorderCategories(idList)
                    .success(function (data, status, headers, config) {
                        vm.reorderProcessing = false;
                        if (data.isSuccess) {
                            vm.reorderProcessingSuccess = true;
                            vm.getCategories();
                        }
                        else {
                            vm.reorderProcessingFailed = true;
                            vm.reorderProcessingSuccess = false;
                            vm.reorderErrorMessage = data.ErrorMessage;
                        }
                    })
                    .error(function (data, status, headers, config) {
                        vm.reorderProcessing = false;
                        vm.reorderProcessingFailed = true;
                        vm.reorderProcessingSuccess = false;
                    vm.reorderErrorMessage = "Could not connect!";
                })
            }

            
        }
        /* ==========================================

         END REORDERING METHODS

       ==========================================*/








        /* ==========================================
               CONTROLLER ACTIVATION
           ==========================================*/

        activate();

        function activate(){

            // Injected variables from the view (via CoreServices/PlatformSettings)
            //Platform --------------------------------------------
            //vm.TrialDaysHold = CoreServiceSettings_Custodian_TrialHoldDays;
            //vm.CustodianFrequencyDescription = CoreServiceSettings_Custodian_FrequencyDescription;
            //vm.UnverifiedAccountsDaysToHold = CoreServiceSettings_Custodian_UnverifiedAccountsDaysToHold;
            //vm.PlatformWorkerFrequencyDescription = CoreServiceSettings_PlatformWorker_FrequencyDescription;

            //Account Roles (used for the logged in Account user, to check Roles accesability
            vm.userRoles = JSON.parse(CoreServiceSettings_AccountUsers_RolesList);

            // Load local profile for the platfor user.
            vm.currentUserProfile = JSON.parse(CurrentUserProfile);

            //For <legal-footer></legal-footer>
            vm.termsLink = termsLink;
            vm.privacyLink = privacyLink;
            vm.acceptableUseLink = acceptableUseLink;
            vm.serviceAgreement = serviceAgreement;
            vm.theCurrentYear = new Date().getFullYear();

            //Debug.trace('---Current profile id:' + vm.currentUserProfile.Id);
            if (vm.currentUserProfile.Id == "")
            {
                //Debug.trace("....User NULL, attempting to redirect to login page (" + vm.currentUserProfile.Id + ")");
                //Log user out if empty
                window.location.replace("/login");

                
            }
            else
            {
                //Debug.trace("....User NOT NULL! (" + vm.currentUserProfile.Id + ")");
                currentUserRoleIndex = vm.userRoles.indexOf(vm.currentUserProfile.Role) //<-- Role will indicate what editing capabilites are available.

                //Update user profile info in case of role updates
                updateCurrentUserProfile();
                // Refresh the profile every 45 seconds (if Role is updated, new editing capabilites will light up for the user)
                setInterval(function () { updateCurrentUserProfile() }, 320000);

                vm.getCategories();
                vm.getAccount();

                //Debug.trace('inventoryIndexController activation complete');
            }



            //Bool: Checks if the users role is allowed
            vm.checkRole = function (lowestRoleAllowed) {

                var allowedIndex = vm.userRoles.indexOf(String(lowestRoleAllowed)); //<-- use Account roles, NOT Platform roles!

                //Debug.trace("Lowest role allowed: '" + lowestRoleAllowed + "'");
                //Debug.trace("Comparing: User: '" + currentUserRoleIndex + "' Allowed: '" + allowedIndex + "'");

                if (currentUserRoleIndex >= allowedIndex) {
                    //Debug.trace("Allowed!");
                    return true;
                }
                else {
                    //Debug.trace("Not allowed!");
                    return false;
                }
            }
        }

    }

})();

