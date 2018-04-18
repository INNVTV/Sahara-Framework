(function () {
    'use strict';

    var controllerId = 'categoryController';

    // Inject into main app module:
    angular.module('app')
        .controller(controllerId, [
            'accountServices',
            'categoryServices',
            'browseServices',
            '$scope',
            '$location',
            '$window',
            '$routeParams',
             categoryController
    ]);

    function categoryController(accountServices, categoryServices, browseServices, $scope, $location, $window, $routeParams) {

        //Instantiate Controller as ViewModel
        var vm = this;

        //Default Properties: =============================
        vm.title = 'categoryController';
        vm.activate = activate;

        vm.loading = true;
        vm.accountNameKey = null;
        vm.apiDomain = null;
        vm.account = null;

        vm.categoryNameKey = $routeParams.categoryNameKey


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
                vm.getCategory();
                setTheme();

            })
            .error(function (data, status, headers, config) {

            })
        }

        /* ==========================================
               Theming
        ==========================================*/

        //var base = { 'background-color': 'blue' }

        //vm.theme = {
        //base : base
        //}

        var setTheme = function () {
            document.body.style.background = "#" + vm.account.themeSettings.colors.background;
            document.body.style.foreground = "#" + vm.account.themeSettings.colors.foreground;
            document.title = vm.account.accountName;
        }

        

        /* ==========================================
               GET CATEGORY TREE
           ==========================================

        vm.categoryTree = null

        vm.getCategoryTree = function () {

            categoryServices.getCategoryTree(vm.accountNameKey, vm.apiDomain)
            .success(function (data, status, headers, config) {

                vm.categoryTree = data.categories;

            })
            .error(function (data, status, headers, config) {

            })
        }*/

        /* ==========================================
               GET CATEGORIES
           ==========================================*/

        vm.category = null

        vm.getCategory = function () {

            categoryServices.getCategory(vm.account.accountNameKey, vm.apiDomain, vm.categoryNameKey)
            .success(function (data, status, headers, config) {

                vm.category = data.category;
                vm.loading = false;
                vm.browseProducts();

            })
            .error(function (data, status, headers, config) {

            })
        }

        /* ==========================================
               GET PRODUCTS
           ==========================================*/

        vm.skip = 0;
        vm.take = 20;
        vm.products = null

        vm.browseProducts = function () {

            browseServices.browseProducts(vm.account.accountNameKey, vm.apiDomain, 'categoryNameKey', vm.categoryNameKey, vm.skip, vm.take)
            .success(function (data, status, headers, config) {

                vm.products = data.items;

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

