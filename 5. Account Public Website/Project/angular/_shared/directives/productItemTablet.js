(function() {
    'use strict';

    // TODO: replace app with your module name
    angular.module('app').directive('productItemTablet', ['$window', productItemTablet]);
    
    function productItemTablet($window) {
        // Usage:
        // 
        // Creates:
        // 
        var directive = {
            link: link,
            replace: true,     //<-- Replace the directive element with the template completly
            restrict: 'AEC',    //<-- Allows use as: Attribute, Element and Class
            templateUrl: '/angular/_shared/templates/productItemTablet.html',      //<-- replaces inline version of ---->   template: '<p>Test Directive</p>'
            scope: false//{
                // variable
                //product: "=",
                //properties: "="

            //}
        };
        return directive;

        function link(scope, element, attrs) {
        }
    }

})();