(function () {
    'use strict';

    var controllerId = 'logsController';

    // Inject into main app module:
    angular.module('app')
        .controller(controllerId, [
            'logsServices',
            'sharedServices',
            'platformServices',
            'accountDetailServices',
             logsController
    ]);

    function logsController(logsServices, sharedServices, platformServices, accountDetailServices) {

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
        var platformRoles = []; //<-- used internally to check role access, must be updated when getting or refreshing the user.
        //--------------------------------------------------------------------

        /* ==========================================
             Core Service Properties
        ==========================================*/

        // vm.TrialHoldDays = null; //<----comes from CoreServices (via local feed)
        //vm.CustodianFrequencyDescription = null; //<----comes from CoreServices (via local feed)
        //vm.UnverifiedAccountsDaysToHold = null; //<----comes from CoreServices (via local feed)
        //vm.PlatformWorkerFrequencyDescription = null; //<----comes from CoreServices (via local feed)



        


        vm.goToAccount = function (accountId) {
            window.location.href = 'account/' + accountId;
        }






        /* ==========================================
            
            LOGS

        ==========================================*/

        vm.platformLog = null;

        vm.platformLogCategory = null;
        vm.platformLogActivity = null;
        vm.platformLogAccountId = null;
        vm.platformLogAccountAttribute = null;
        vm.platformLogUserId = null;


        vm.platformLogRecordResultCounts = [
            { label: '50 records',  value: 50 },
            { label: '100 records', value: 100 },
            { label: '250 records', value: 250 },
            { label: '500 records', value: 500 },
            { label: '750 records', value: 750 },
            { label: '1,000 records', value: 1000 },
        ]

        vm.platformLogCategories = [];
        vm.platformLogActivities = [];


        vm.currentplatformLogRecordResultCount = vm.platformLogRecordResultCounts[0];
        vm.updateplatformLogRecordResultCount = function (count) {
            vm.currentplatformLogRecordResultCount = count;
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

                    Debug.trace(buttonName + " clicked");

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

        vm.platformLog = null;

        vm.platformLogCategory = null;
        vm.platformLogActivity = null;
        vm.platformLogUser = null;
        //vm.platformLogObjectId = null;

        // Adjust filters ------------

        vm.updateCurrentLogFilterCategory = function (category) {
            vm.platformLogCategory = category
        }

        vm.updateCurrentLogFilterActivity = function (activity) {
            vm.platformLogActivity = activity
        }

        vm.updateCurrentLogFilterUser = function (user) {
            vm.platformLogUser = user
        }


        /*============================================================
            LOG METHODS
        =======================================================*/

        vm.activateLogsSection = function () {
            // Load initial log set
            vm.getPlatformLog(vm.currentplatformLogRecordResultCount.value);

            // Load Category List
            logsServices.getPlatformLogCategories()
            .success(function (data, status, headers, config) {
                vm.platformLogCategories = data;
                vm.platformLogCategory = data[0];
            })
            .error(function (data, status, headers, config) {

            })


            // Load Activity List
            logsServices.getPlatformLogActivities()
            .success(function (data, status, headers, config) {
                vm.platformLogActivities = data;
                vm.platformLogActivity = data[0];
            })
            .error(function (data, status, headers, config) {

            })

            // Load Platform Users
            platformServices.getUsers()
            .success(function (data, status, headers, config) {

                // Load user dropdown list
                vm.platformUsers = data;
                // Load user dropdown list default
                vm.platformLogUser = vm.platformUsers[0];

                
            })
            .error(function (data, status, headers, config) {
                vm.platformLogLoading = false;
            })

           

        }

        // Update Logs ----------------------------

        vm.updateplatformLog = function () {
            if (vm.logsSubMenu.allButton) {
                vm.getPlatformLog(vm.currentplatformLogRecordResultCount.value);
            }
            else if (vm.logsSubMenu.categoryButton) {
                vm.getPlatformLogByCategory(vm.platformLogCategory, vm.currentplatformLogRecordResultCount.value);
            }
            else if (vm.logsSubMenu.activityButton) {
                vm.getPlatformLogByActivity(vm.platformLogActivity, vm.currentplatformLogRecordResultCount.value);
            }
            else if (vm.logsSubMenu.accountButton) {
                vm.getPlatformLogByAccount(vm.platformLogAccountAttribute, vm.currentplatformLogRecordResultCount.value);
            }
            else if (vm.logsSubMenu.userButton) {
                vm.getPlatformLogByUser(vm.platformLogUser.Id, vm.currentplatformLogRecordResultCount.value);
            }

        }



        // Basic Log Calls ------------------------

        vm.getPlatformLog = function (count) {

            vm.platformLogLoading = true;

            logsServices.getPlatformLog(count)
            .success(function (data, status, headers, config) {
                vm.platformLog = data;
                vm.platformLogLoading = false;
            })
            .error(function (data, status, headers, config) {
                vm.platformLogLoading = false;
            })
        }

        vm.getPlatformLogByCategory = function (category, count) {

            vm.platformLogLoading = true;

            logsServices.getPlatformLogByCategory(category, count)
            .success(function (data, status, headers, config) {

                vm.platformLog = data;
                vm.platformLogLoading = false;
            })
            .error(function (data, status, headers, config) {
                vm.platformLogLoading = false;
            })
        }

        vm.getPlatformLogByActivity = function (activity, count) {

            vm.platformLogLoading = true;

            logsServices.getPlatformLogByActivity(activity, count)
            .success(function (data, status, headers, config) {

                vm.platformLog = data;
                vm.platformLogLoading = false;
            })
            .error(function (data, status, headers, config) {
                vm.platformLogLoading = false;
            })
        }

        vm.getPlatformLogByAccount = function (accountAttribute, count) {

            vm.platformLogLoading = true;

            Debug.trace(accountAttribute);

            //Get the account using the attribute
            accountDetailServices.getAccount(accountAttribute)
                .success(function (data, status, headers, config) {

                //Extract the id:
                var accountId = data.AccountID;

                //Update Logs
                logsServices.getPlatformLogByAccount(accountId, count)
                    .success(function (data, status, headers, config) {

                        vm.platformLog = data;
                        vm.platformLogLoading = false;
                    })
                    .error(function (data, status, headers, config) {
                        vm.platformLogLoading = false;
                    })

                })
            .error(function (data, status, headers, config) {
                vm.platformLogLoading = false;
            })
        }

        vm.getPlatformLogByUser = function (userId, count) {

            vm.platformLogLoading = true;

            logsServices.getPlatformLogByUser(userId, count)
            .success(function (data, status, headers, config) {

                vm.platformLog = data;
                vm.platformLogLoading = false;
            })
            .error(function (data, status, headers, config) {
                vm.platformLogLoading = false;
            })
        }


        vm.getLogDetail = function (index, reset) {

            vm.logDetail = vm.platformLog[index];

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

            Debug.trace("Refreshing user profile...");

            sharedServices.getCurrentUserProfile()
            .success(function (data, status, headers, config) {

                vm.currentUserProfile = data; //Used to determine what is shown in the view based on user Role.
                currentUserRoleIndex = vm.platformRoles.indexOf(data.Role) //<-- use PLATFORM roles, NOT platform roles!

                Debug.trace("Profile refreshed!");
                Debug.trace("Role index = " + currentUserRoleIndex);

            })
                .error(function (data, status, headers, config) {


                })

        }


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

            //Platform Roles (used for the logged in Platform user, to check Roles accesability
            vm.platformRoles = JSON.parse(CoreServiceSettings_PlatformUsers_RolesList);

            // Load local profile for the platfor user.
            vm.currentUserProfile = JSON.parse(CurrentUserProfile);
            currentUserRoleIndex = vm.platformRoles.indexOf(vm.currentUserProfile.Role) //<-- Role will indicate what editing capabilites are available.
            // Refresh the profile every 45 seconds (if Role is updated, new editing capabilites will light up for the user)
            setInterval(function () { updateCurrentUserProfile() }, 150000);

            vm.activateLogsSection();

            Debug.trace('logsIndexController activation complete');



            //Bool: Checks if the users role is allowed
            vm.checkRole = function (lowestRoleAllowed) {

                var allowedIndex = vm.platformRoles.indexOf(String(lowestRoleAllowed)); //<-- use Platform roles, NOT platform roles!

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

