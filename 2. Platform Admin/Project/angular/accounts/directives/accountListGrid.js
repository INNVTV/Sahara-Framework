(function() {
    'use strict';

    // TODO: replace app with your module name
    angular.module('app')
        .directive('accountListGrid', ['$window', accountListGrid]);
    
    function accountListGrid ($window) {
        // Usage:
        // 
        // Creates:
        // 
        var directive = {
            link: link,
            replace: true,      //<-- Replace the directive element with the template completly
            restrict: 'AEC',    //<-- Allows use as: Attribute, Element and Class
            templateUrl: '/angular/accounts/templates/accountListGrid.html' //<-- replaces inline version of ---->   template: '<p>Test Directive</p>'
        };
        return directive;

        // The link function is mainly used for attaching event listeners to DOM elements, watching model properties for changes, and updating the DOM. 
        function link(scope, element, attrs) {
        }
    }

})();