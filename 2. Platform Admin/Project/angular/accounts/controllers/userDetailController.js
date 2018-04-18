(function () {
    'use strict';

    var controllerId = 'userDetailController';

    // TODO: replace app with your module name
    angular.module('app')
        .controller(controllerId,
        [
            'userDetailServices',
           '$routeParams',
            userDetailController
        ]);

    function userDetailController(userDetailController, $routeParams) {

        //Instantiate Controller as ViewModel
        var vm = this;

        // Default Properties===============================
        vm.activate = activate;
        vm.title = 'userDetailController';

        // Debug Methods ===================================
        // wrap console.log() within the "Debug" namespace
        //    -->  Logs can be removed during minification
        var Debug = {};
        Debug.trace = function (message) {
            console.log(message);
        };


        // Controller Properties

        vm.routeId = $routeParams.id; //$route.current.params.id; 
        vm.showGetUserLoading = true;
        vm.user = null;


        // Controller Methods

        vm.getUser = function () {

            Debug.trace('Getting user details...');

        vm.showGetUserLoading = true;

            userDetailServices.getUser(vm.routeId)
                    .success(function (data, status, headers, config) {
                        vm.user = data;

                        //Update UTC to local time on the client side:
                        vm.user.CreatedDate = new Date(vm.user.CreatedDate.toLocaleString());

                        vm.showGetUserLoading = false;


                    })
                    .error(function (data, status, headers, config) {
                        //
                    })
        }



        activate();

        function activate() {

            Debug.trace('UserDetail Controller activation complete');
            vm.getUser();

        }
    }
})();
