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
                when("/marketplace", { templateUrl: "/angular/marketplace/partials/marketplaceIndex.html", caseInsensitiveMatch: true }).
                when("/marketplace/:id", { templateUrl: "/angular/marketplace/partials/marketplaceDetail.html", caseInsensitiveMatch: true })   //<-- :id is route parameter


            // Hide #Hashtag from route URLs on browsers that allow it
            if (window.history && window.history.pushState) {
                $locationProvider.html5Mode(true);
            }
        });

    // Execute bootstrapping code and any dependencies.

    // inject services as needed:

    app.run(['$q', '$rootScope',
        function ($q, $rootScope) {



        }]);

})();