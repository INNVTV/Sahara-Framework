(function () {
    'use strict';

    var id = 'app';

    // TODO: Inject modules as needed.
    var app = angular.module('app', [
        // Angular modules 
        //'ngAnimate',        // animations
        'ngRoute',           // routing
        'ngLocationUpdate',    //plugin to allow for route updates without refresh
        //'wu.masonry',
        // Custom modules 

        // 3rd Party Modules
        //'ui.bootstrap',
        //'angularMoment'
    ])

        //Start....Additional filter needed for iFrame embeds of Google Maps:
        .controller('filters-module', [])
        .filter('trustAsResourceUrl', ['$sce', function ($sce) {
            return function (val) {
                return $sce.trustAsResourceUrl(val);
            };
        }]) //...End (Additional filter needed for iFrame embeds of Google Maps)

        .config(function ($routeProvider, $locationProvider) {

            $routeProvider.

                when("/", { templateUrl: "/angular/browse/partials/browse.html", caseInsensitiveMatch: true }).
                //when("/:categoryNameKey", { templateUrl: "/angular/browse/partials/browse.html", caseInsensitiveMatch: true }).
                //when("/:categoryNameKey/:subcategoryNameKey", { templateUrl: "/angular/browse/partials/browse.html", caseInsensitiveMatch: true }).
                //when("/:categoryNameKey/:subcategoryNameKey/:subsubcategoryNameKey", { templateUrl: "/angular/browse/partials/browse.html", caseInsensitiveMatch: true }).
                //when("/:categoryNameKey/:subcategoryNameKey/:subsubcategoryNameKey/:subsubsubcategoryNameKey", { templateUrl: "/angular/browse/partials/browse.html", caseInsensitiveMatch: true }).

                when("/browse", { templateUrl: "/angular/browse/partials/browse.html", caseInsensitiveMatch: true }).
                when("/browse/:categoryNameKey", { templateUrl: "/angular/browse/partials/browse.html", caseInsensitiveMatch: true }).
                when("/browse/:categoryNameKey/:subcategoryNameKey", { templateUrl: "/angular/browse/partials/browse.html", caseInsensitiveMatch: true }).
                when("/browse/:categoryNameKey/:subcategoryNameKey/:subsubcategoryNameKey", { templateUrl: "/angular/browse/partials/browse.html", caseInsensitiveMatch: true }).
                when("/browse/:categoryNameKey/:subcategoryNameKey/:subsubcategoryNameKey/:subsubsubcategoryNameKey", { templateUrl: "/angular/browse/partials/browse.html", caseInsensitiveMatch: true }).
                
                when("/item/:categoryNameKey/:productNameKey", { templateUrl: "/angular/browse/partials/browse.html", caseInsensitiveMatch: true }).
                when("/item/:categoryNameKey/:subcategoryNameKey/:productNameKey", { templateUrl: "/angular/browse/partials/browse.html", caseInsensitiveMatch: true }).
                when("/item/:categoryNameKey/:subcategoryNameKey/:subsubcategoryNameKey/:productNameKey", { templateUrl: "/angular/browse/partials/browse.html", caseInsensitiveMatch: true }).
                when("/item/:categoryNameKey/:subcategoryNameKey/:subsubcategoryNameKey/:subsubsubcategoryNameKey/:productNameKey", { templateUrl: "/angular/browse/partials/browse.html", caseInsensitiveMatch: true })



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