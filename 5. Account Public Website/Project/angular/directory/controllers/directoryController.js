(function () {
    'use strict';

    var controllerId = 'directoryController';

    // Inject into main app module:
    angular.module('app')
        .controller(controllerId, [
            'accountServices',
            'categoryServices',
            '$scope',
            '$location',
            '$window',
            '$routeParams',
             directoryController
    ]);

    function directoryController(accountServices, categoryServices, $scope, $location, $window, $routeParams) {

        //Instantiate Controller as ViewModel
        var vm = this;

        //Default Properties: =============================
        vm.title = 'directoryController';
        vm.activate = activate;

        vm.loading = true;
        vm.accountNameKey = null;
        vm.apiDomain = null;
        vm.account = null;




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

        /* ==========================================
               RE-ROUTING
        ==========================================*/

        vm.updateRoute = function(path)
        {
            //window.location.replace(path);
            $window.location.href = path;
            //$location.path(path)
        }

        /* ==========================================
               GET ACCOUNT SETTINGS
           ==========================================*/

        

        vm.getAccountSettings = function () {

            accountServices.getAccount(vm.accountNameKey, vm.apiDomain)
            .success(function (data, status, headers, config) {
                
                vm.account = data.account;
                document.title = vm.account.accountName;
                vm.getCategoryTree();                
                //setTheme();

            })
            .error(function (data, status, headers, config) {

            })
        }

        /* ==========================================
               Theming
        ==========================================

        //var base = { 'background-color': 'blue' }

        //vm.theme = {
        //base : base
        //}

        var setTheme = function () {
            document.body.style.background = "#" + vm.account.themeSettings.colors.background;
            document.body.style.foreground = "#" + vm.account.themeSettings.colors.foreground;
            document.title = vm.account.accountName;
        }
       
*/
        /* ==========================================
               GET CATEGORY TREE
           ==========================================*/

        vm.categories = null;
        vm.depth = null;

        vm.getCategoryTree = function () {

            categoryServices.getCategoryTree(vm.account.accountNameKey, vm.apiDomain)
                .success(function (data, status, headers, config) {
                    
                    vm.categories = data.categories;
                    vm.depth = data.categoryDepth;
                })
                .error(function (data, status, headers, config) {
            
                })


        }



        /* ==========================================
               CONTROLLER ACTIVATION
           ==========================================*/

        activate();

        function activate(){

            vm.accountNameKey = AccountNameKey;
            vm.apiDomain = ApiDomain;

            vm.getAccountSettings();

        }

    }

})();

