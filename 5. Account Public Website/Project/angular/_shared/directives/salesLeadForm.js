(function() {
    'use strict';

    // TODO: replace app with your module name
    angular.module('app').directive('salesLeadForm', ['$window', salesLeadForm]);
    
    function salesLeadForm($window) {
        // Usage:
        // 
        // Creates:
        // 
        var directive = {
            link: link,
            replace: true,     //<-- Replace the directive element with the template completly
            restrict: 'AEC',    //<-- Allows use as: Attribute, Element and Class
            templateUrl: '/angular/_shared/templates/salesLeadForm.html',      //<-- replaces inline version of ---->   template: '<p>Test Directive</p>'
        };
        return directive;

        function link(scope, element, attrs) {
        }
    }

})();