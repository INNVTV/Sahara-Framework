(function() {
    'use strict';

    // TODO: replace app with your module name
    angular.module('app').directive('categoryItemTablet', ['$window', categoryItemTablet]);
    
    function categoryItemTablet($window) {
        // Usage:
        // 
        // Creates:
        // 
        var directive = {
            link: link,
            replace: true,     //<-- Replace the directive element with the template completly
            restrict: 'AEC',    //<-- Allows use as: Attribute, Element and Class
            templateUrl: '/angular/_shared/templates/categoryItemTablet.html',      //<-- replaces inline version of ---->   template: '<p>Test Directive</p>'
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