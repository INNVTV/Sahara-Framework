(function () {
    'use strict';

    var controllerId = 'dataInjectionDetailController';

    // TODO: replace app with your module name
    angular.module('app')
        .controller(controllerId,
        [
            'dataInjectionDetailServices',
            'sharedServices',
           '$routeParams',
            dataInjectionDetailController
        ]);

    function dataInjectionDetailController(dataInjectionDetailServices, sharedServices, $routeParams) {

        //Instantiate Controller as ViewModel
        var vm = this;

        // Default Properties===============================
        vm.activate = activate;
        vm.title = 'dataInjectionDetailController';


        // Debug Methods ===================================
        // wrap console.log() within the "Debug" namespace
        //    -->  Logs can be removed during minification
        var Debug = {};
        Debug.trace = function (message) {
            console.log(message);
        };


        // Controller Properties

        //Account User:
        vm.currentUserProfile = null;
        var currentUserRoleIndex = null //<-- used internally to check role access, must be updated when getting or refreshing the user.
        var userRoles = []; //<-- used internally to check role access, must be updated when getting or refreshing the user.
        //--------------------------------------------------------------------


        vm.routeId = $routeParams.id; //$route.current.params.id; 


        /* ==========================================
           Core Service Properties
          ==========================================*/

       // vm.TrialHoldDays = null; //<----comes from CoreServices (via local feed)
        //vm.CustodianFrequencyDescription = null; //<----comes from CoreServices (via local feed)
        //vm.UnverifiedAccountsDaysToHold = null; //<----comes from CoreServices (via local feed)
        //vm.PlatformWorkerFrequencyDescription = null; //<----comes from CoreServices (via local feed)






        /* ==========================================
           Base Controller Methods
          ==========================================*/





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
            HELPER FUNCTIONS
        ==========================================*/








        /* ==========================================
           CONTROLLER ACTIVATION
        ==========================================*/


        activate();

        function activate() {

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
            currentUserRoleIndex = vm.userRoles.indexOf(vm.currentUserProfile.Role) //<-- Role will indicate what editing capabilites are available.

            if (vm.currentUserProfile.Id == "") {
                //Log user out if empty
                window.location.replace("/login");
            }

            //Update user profile info in case of role updates
            updateCurrentUserProfile();
            // Refresh the profile every 45 seconds (if Role is updated, new editing capabilites will light up for the user)
            setInterval(function () { updateCurrentUserProfile() }, 320000);


            //Debug.trace('DataInjectionDetailController activation complete');



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
