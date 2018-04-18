(function () {
    'use strict';

    var id = 'app';

    // TODO: Inject modules as needed.
    var app = angular.module('app', [
        // Angular modules 
        //'ngAnimate',        // animations
        'ngRoute',           // routing
        'ngLocationUpdate',    //plugin to allow for route updates without refresh
        // Custom modules 

        // 3rd Party Modules
        'ui.bootstrap',
        'angularMoment',
        'ui.sortable'
        //'ngMap'
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
                when("/", { templateUrl: "/angular/inventory/partials/inventoryIndex.html", caseInsensitiveMatch: true }).
                when("/inventory", { templateUrl: "/angular/inventory/partials/inventoryIndex.html", caseInsensitiveMatch: true }).
                //when("/inventory/:id", { templateUrl: "/angular/inventory/partials/inventoryDetail.html", caseInsensitiveMatch: true }).  //<-- :id is route parameter
                when("/categories", { templateUrl: "/angular/inventory/partials/categoriesIndex.html", caseInsensitiveMatch: true }).
                when("/category/:attribute", { templateUrl: "/angular/inventory/partials/categoryDetail.html", caseInsensitiveMatch: true }).   //<-- :attribute is route parameter
                //All subcategorizations share a single view
                when("/subcategory/:categoryAttribute/:subcategoryAttribute", { templateUrl: "/angular/inventory/partials/subcategoryDetail.html", caseInsensitiveMatch: true }).   //<-- :attribute is route parameter
                when("/subsubcategory/:categoryAttribute/:subcategoryAttribute/:subsubcategoryAttribute", { templateUrl: "/angular/inventory/partials/subsubcategoryDetail.html", caseInsensitiveMatch: true }).
                when("/subsubsubcategory/:categoryAttribute/:subcategoryAttribute/:subsubcategoryAttribute/:subsubsubcategoryAttribute", { templateUrl: "/angular/inventory/partials/subsubsubcategoryDetail.html", caseInsensitiveMatch: true }).
                //Must use every variation of "product" route
                when("/item/:categoryNameKey/:productNameKey", { templateUrl: "/angular/inventory/partials/productDetail.html", caseInsensitiveMatch: true }).
                when("/item/:categoryNameKey/:subcategoryNameKey/:productNameKey", { templateUrl: "/angular/inventory/partials/productDetail.html", caseInsensitiveMatch: true }).
                when("/item/:categoryNameKey/:subcategoryNameKey/:subsubcategoryNameKey/:productNameKey", { templateUrl: "/angular/inventory/partials/productDetail.html", caseInsensitiveMatch: true }).
                when("/item/:categoryNameKey/:subcategoryNameKey/:subsubcategoryNameKey/:subsubsubcategoryNameKey/:productNameKey", { templateUrl: "/angular/inventory/partials/productDetail.html", caseInsensitiveMatch: true })

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