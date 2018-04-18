(function () {
    'use strict';

    var controllerId = 'scaffoldIndexController';

    // Inject into main app module:
    angular.module('app')
        .controller(controllerId, [
            'scaffoldIndexServices',
            'scaffoldIndexModels',
            'sharedServices',
             scaffoldIndexController
    ]);

    function scaffoldIndexController(scaffoldIndexServices, scaffoldIndexModels, sharedServices) {

        //Instantiate Controller as ViewModel
        var vm = this;

        //Default Properties: =============================
        vm.title = 'scaffoldIndexController';
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



        /* ==========================================
                Controller Properties
        ==========================================*/

        vm.showLoader = true;

        vm.Property1 = null;

        /* ==========================================
               Controller Models
       ==========================================*/

        vm.Property2 =
             {
                 value: null,
                 label: null,

                 details: {
                     name: "the name",
                     id: null
                 }

             }


        vm.objectDetail =
                {
                    ID: null,
                    Name: null,
                    InternalArray: [],

                    UpdateName: function (name) {
                        this.Name = name;
                    },

                    updateName: {
                        editing: false,
                        processing: false,
                        complete: false,
                        isSuccess: false,
                        message: null
                    },
                    
                    reset: function () {
                        ID: null;
                        Name: null;
                    }

                }


        /* ==========================================
               Base Controller Methods
        ==========================================*/

        vm.hideLoader = function()
        {
            vm.showLoader = false;
        }

        // Get Methods ---------------------------

        vm.getData = function () {

            Debug.trace("Getting data...");

            scaffoldIndexServices.getData(id)
            .success(function (data, status, headers, config) {

                if (data.isSuccess) {

                }
                else {

                }

            })
            .error(function (data, status, headers, config) {

            })
        }


        // Update Methods ---------------------------

        vm.updateObject = function () {

            vm.objectDetail.updateName.processing = true;

            Debug.trace("Updating data...");

            scaffoldIndexServices.updateObject(id)
            .success(function (data, status, headers, config) {

                vm.objectDetail.updateName.processing = false;
                vm.objectDetail.updateName.complete = true;

                if (data.isSuccess) {

                    vm.objectDetail.updateName.isSuccess = true;
                    vm.objectDetail.updateName.message = "This object has been updated.";

                }
                else {
                    
                    vm.objectDetail.updateName.isSuccess = false;
                    vm.objectDetail.updateName.message = data.ErrorMessage;
                }

            })
            .error(function (data, status, headers, config) {

                vm.objectDetail.updateName.processing = false;
                vm.objectDetail.updateName.isSuccess = false;
                vm.objectDetail.updateName.complete = true;
                vm.objectDetail.updateName.message = "An error occurred while attempting to use the service...";

            })
        }
 


        /* ==========================================
           CURRENT USER PROFILE
       ==========================================*/

        function updateCurrentUserProfile() {

            Debug.trace("Refreshing user profile...");

            sharedServices.getCurrentUserProfile()
            .success(function (data, status, headers, config) {

                vm.currentUserProfile = data; //Used to determine what is shown in the view based on user Role.
                currentUserRoleIndex = vm.platformRoles.indexOf(data.Role) //<-- use PLATFORM roles, NOT ACCOUNT roles!

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
            setInterval(function () { updateCurrentUserProfile() }, 45000);


            vm.Property1 = "Property One";

            vm.Property2.label = "test";
            vm.Property2.value = 1;

            Debug.trace('scaffoldIndexController activation complete');



            //Bool: Checks if the users role is allowed
            vm.checkRole = function (lowestRoleAllowed) {

                var allowedIndex = vm.platformRoles.indexOf(String(lowestRoleAllowed)); //<-- use Platform roles, NOT account roles!

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

