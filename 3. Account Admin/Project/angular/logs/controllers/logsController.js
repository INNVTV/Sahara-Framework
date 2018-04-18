(function () {
    'use strict';

    var controllerId = 'logsController';

    // Inject into main app module:
    angular.module('app')
        .controller(controllerId, [
            'accountServices',
            'sharedServices',
            'accountUserServices',
             logsController
    ]);

    function logsController(accountServices, sharedServices, accountUserServices) {

        //Instantiate Controller as ViewModel
        var vm = this;

        //Default Properties: =============================
        vm.title = 'logsController';
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


        //Platform User:
        vm.currentUserProfile = null;
        var currentUserRoleIndex = null //<-- used internally to check role access, must be updated when getting or refreshing the user.
        var accountRoles = []; //<-- used internally to check role access, must be updated when getting or refreshing the user.
        //--------------------------------------------------------------------

        /* ==========================================
             Core Service Properties
        ==========================================*/

        // vm.TrialHoldDays = null; //<----comes from CoreServices (via local feed)
        //vm.CustodianFrequencyDescription = null; //<----comes from CoreServices (via local feed)
        //vm.UnverifiedAccountsDaysToHold = null; //<----comes from CoreServices (via local feed)
        //vm.AccountWorkerFrequencyDescription = null; //<----comes from CoreServices (via local feed)



        


        vm.goToAccount = function (accountId) {
            window.location.href = 'account/' + accountId;
        }






        /* ==========================================
            
            LOGS

        ==========================================*/

        vm.accountLog = null;

        vm.accountLogCategory = null;
        vm.accountLogActivity = null;
        vm.accountLogAccountId = null;
        vm.accountLogAccountAttribute = null;
        vm.accountLogUserId = null;


        vm.accountLogRecordResultCounts = [
            { label: '50 records',  value: 50 },
            { label: '100 records', value: 100 },
            { label: '250 records', value: 250 },
            { label: '500 records', value: 500 },
            { label: '750 records', value: 750 },
            { label: '1,000 records', value: 1000 },
        ]

        vm.accountLogCategories = [];
        vm.accountLogActivities = [];


        vm.currentaccountLogRecordResultCount = vm.accountLogRecordResultCounts[0];
        vm.updateaccountLogRecordResultCount = function (count) {
            vm.currentaccountLogRecordResultCount = count;
        }


        /*============================================================
                    LOGS SUBMENU
        =======================================================*/

        vm.logsSubMenu =
            {
                allButton: true,
                categoryButton: false,
                activityButton: false,
                accountButton: false,
                userButton: false,

                update: function (buttonName) {

                    //Debug.trace(buttonName + " clicked");

                    if (buttonName == 'all') {
                        this.allButton = true,
                        this.categoryButton = false,
                        this.activityButton = false,
                        this.accountButton = false,
                        this.userButton = false
                    }
                    if (buttonName == 'category') {
                        this.allButton = false,
                        this.categoryButton = true,
                        this.activityButton = false,
                        this.accountButton = false,
                        this.userButton = false
                    }
                    if (buttonName == 'activity') {
                        this.allButton = false,
                        this.categoryButton = false,
                        this.activityButton = true,
                        this.accountButton = false,
                        this.userButton = false
                    }
                    if (buttonName == 'account') {
                        this.allButton = false,
                        this.categoryButton = false,
                        this.activityButton = false,
                        this.accountButton = true,
                        this.userButton = false
                    }
                    if (buttonName == 'user') {
                        this.allButton = false,
                        this.categoryButton = false,
                        this.activityButton = false,
                        this.accountButton = false,
                        this.userButton = true
                    }
                },
            }

        vm.accountLog = null;

        vm.accountLogCategory = null;
        vm.accountLogActivity = null;
        vm.accountLogUser = null;
        //vm.accountLogObjectId = null;

        // Adjust filters ------------

        vm.updateCurrentLogFilterCategory = function (category) {
            vm.accountLogCategory = category
        }

        vm.updateCurrentLogFilterActivity = function (activity) {
            vm.accountLogActivity = activity
        }

        vm.updateCurrentLogFilterUser = function (user) {
            vm.accountLogUser = user
        }


        /*============================================================
            LOG METHODS
        =======================================================*/

        vm.activateLogsSection = function () {
            // Load initial log set
            vm.getAccountLog(vm.currentaccountLogRecordResultCount.value);

            // Load Category List
            accountServices.getAccountLogCategories()
            .success(function (data, status, headers, config) {
                vm.accountLogCategories = data;
                vm.accountLogCategory = data[0];
            })
            .error(function (data, status, headers, config) {

            })


            // Load Activity List
            accountServices.getAccountLogActivities()
            .success(function (data, status, headers, config) {
                vm.accountLogActivities = data;
                vm.accountLogActivity = data[0];
            })
            .error(function (data, status, headers, config) {

            })

            // Load Account Users
            accountUserServices.getUsers()
            .success(function (data, status, headers, config) {

                // Load user dropdown list
                vm.accountUsers = data;
                // Load user dropdown list default
                vm.accountLogUser = vm.accountUsers[0];

                
            })
            .error(function (data, status, headers, config) {
                vm.accountLogLoading = false;
            })

           

        }

        // Update Logs ----------------------------

        vm.updateaccountLog = function () {
            if (vm.logsSubMenu.allButton) {
                vm.getAccountLog(vm.currentaccountLogRecordResultCount.value);
            }
            else if (vm.logsSubMenu.categoryButton) {
                vm.getAccountLogByCategory(vm.accountLogCategory, vm.currentaccountLogRecordResultCount.value);
            }
            else if (vm.logsSubMenu.activityButton) {
                vm.getAccountLogByActivity(vm.accountLogActivity, vm.currentaccountLogRecordResultCount.value);
            }
            else if (vm.logsSubMenu.accountButton) {
                vm.getAccountLogByAccount(vm.accountLogAccountAttribute, vm.currentaccountLogRecordResultCount.value);
            }
            else if (vm.logsSubMenu.userButton) {
                vm.getAccountLogByUser(vm.accountLogUser.Id, vm.currentaccountLogRecordResultCount.value);
            }

        }



        // Basic Log Calls ------------------------

        vm.getAccountLog = function (count) {

            vm.accountLogLoading = true;

            accountServices.getAccountLog(count)
            .success(function (data, status, headers, config) {
                vm.accountLog = data;
                vm.accountLogLoading = false;
            })
            .error(function (data, status, headers, config) {
                vm.accountLogLoading = false;
            })
        }

        vm.getAccountLogByCategory = function (category, count) {

            vm.accountLogLoading = true;

            accountServices.getAccountLogByCategory(category, count)
            .success(function (data, status, headers, config) {

                vm.accountLog = data;
                vm.accountLogLoading = false;
            })
            .error(function (data, status, headers, config) {
                vm.accountLogLoading = false;
            })
        }

        vm.getAccountLogByActivity = function (activity, count) {

            vm.accountLogLoading = true;

            accountServices.getAccountLogByActivity(activity, count)
            .success(function (data, status, headers, config) {

                vm.accountLog = data;
                vm.accountLogLoading = false;
            })
            .error(function (data, status, headers, config) {
                vm.accountLogLoading = false;
            })
        }

        vm.getAccountLogByUser = function (userId, count) {

            vm.accountLogLoading = true;

            accountServices.getAccountLogByUser(userId, count)
            .success(function (data, status, headers, config) {

                vm.accountLog = data;
                vm.accountLogLoading = false;
            })
            .error(function (data, status, headers, config) {
                vm.accountLogLoading = false;
            })
        }


        vm.getLogDetail = function (index, reset) {

            vm.logDetail = vm.accountLog[index];

            //vm.logDetail.Object = JSON.stringify(vm.logDetail.Object, null, 4);
            //vm.logDetail.Objec = angular.fromJson(vm.logDetail.Object);
            //vm.paymentDetail.Index = index;
            /*
            vm.logDetail.Object = JSON.stringify(vm.logDetail.Object);
            vm.logDetail.Object = vm.logDetail.Object.replace(/[\"]/g, '\\"')
      .replace(/[\\]/g, '\\\\')
      .replace(/[\/]/g, '\\/')
      .replace(/[\b]/g, '\\b')
      .replace(/[\f]/g, '\\f')
      .replace(/[\n]/g, '\\n')
      .replace(/[\r]/g, '\\r')
      .replace(/[\t]/g, '\\t');*/

        }














 


        /* ==========================================
           CURRENT USER PROFILE
       ==========================================*/

        function updateCurrentUserProfile() {

            //Debug.trace("Refreshing user profile...");

            sharedServices.getCurrentUserProfile()
            .success(function (data, status, headers, config) {

                vm.currentUserProfile = data; //Used to determine what is shown in the view based on user Role.
                currentUserRoleIndex = vm.accountRoles.indexOf(data.Role) //<-- use PLATFORM roles, NOT platform roles!

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
               CONTROLLER ACTIVATION
           ==========================================*/

        activate();

        function activate(){

            // Injected variables from the view (via CoreServices/AccountSettings)
            //Account --------------------------------------------
            //vm.TrialDaysHold = CoreServiceSettings_Custodian_TrialHoldDays;
            //vm.CustodianFrequencyDescription = CoreServiceSettings_Custodian_FrequencyDescription;
            //vm.UnverifiedAccountsDaysToHold = CoreServiceSettings_Custodian_UnverifiedAccountsDaysToHold;
            //vm.AccountWorkerFrequencyDescription = CoreServiceSettings_AccountWorker_FrequencyDescription;

            //Account Roles (used for the logged in Account user, to check Roles accesability
            vm.accountRoles = JSON.parse(CoreServiceSettings_AccountUsers_RolesList);

            // Load local profile for the platfor user.
            vm.currentUserProfile = JSON.parse(CurrentUserProfile);
            currentUserRoleIndex = vm.accountRoles.indexOf(vm.currentUserProfile.Role) //<-- Role will indicate what editing capabilites are available.

            if (vm.currentUserProfile.Id == "") {
                //Log user out if empty
                window.location.replace("/login");
            }

            //Update user profile info in case of role updates
            updateCurrentUserProfile();
            // Refresh the profile every 45 seconds (if Role is updated, new editing capabilites will light up for the user)
            setInterval(function () { updateCurrentUserProfile() }, 320000);

            vm.activateLogsSection();

            //Debug.trace('logsIndexController activation complete');



            //Bool: Checks if the users role is allowed
            vm.checkRole = function (lowestRoleAllowed) {

                var allowedIndex = vm.accountRoles.indexOf(String(lowestRoleAllowed)); //<-- use Account roles, NOT platform roles!

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

