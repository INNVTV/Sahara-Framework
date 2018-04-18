(function () {
    'use strict';

    var id = 'app';

    // TODO: Inject modules as needed.
    var app = angular.module('app', [
        // Angular modules 
        //'ngAnimate',        // animations
        'ngRoute',           // routing

        // Custom modules 

        // 3rd Party Modules
        'ui.bootstrap',
        'angularMoment'
    ])
        .config(function ($routeProvider, $locationProvider) {

            $routeProvider.
                when("/accounts", { templateUrl: "/angular/accounts/partials/accountList.html", caseInsensitiveMatch: true }).
                when("/account/:id", { templateUrl: "/angular/accounts/partials/accountDetail.html", caseInsensitiveMatch: true })   //<-- :id is route parameter
            //when("/detail/:id", { templateUrl: "/angular/accounts/partials/accountDetail.html", controller: accountDetailController }) //<-- can also inject controllers thusly
            //, caseInsensitiveMatch: true //<-- turns off case sensitivity on each route

            // Hide #Hashtag from route URLs on browsers that allow it
            if (window.history && window.history.pushState) {
                $locationProvider.html5Mode(true);
            }
        });

    // Execute bootstrapping code and any dependencies.

    // inject services as needed:

    //app.controller('accountListController', accountListController);
    //app.service('accountServices', accountServices);

    app.run(['$q', '$rootScope',
        function ($q, $rootScope) {

        }]);

})();

