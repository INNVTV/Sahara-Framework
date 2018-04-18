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
        //'infinite-scroll'
    ])
        .config(function ($routeProvider, $locationProvider) {

            $routeProvider.
                when("/search", { templateUrl: "/angular/search/partials/searchIndex.html", caseInsensitiveMatch: true }).
                when("/search/:searchMode", { templateUrl: "/angular/search/partials/searchIndex.html", caseInsensitiveMatch: true })   //<-- :id is route parameter


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