(function () {
    'use strict';

    // TODO: replace app with your module name
    angular.module('app').directive('aboutImageFormatsModal', ['$window', aboutImageFormatsModal]);
    
    function aboutImageFormatsModal($window) {
        // Usage:
        // 
        // Creates:
        // 
        var directive = {
            link: link,
            replace: true,     //<-- Replace the directive element with the template completly
            restrict: 'AEC',    //<-- Allows use as: Attribute, Element and Class
            templateUrl: '/angular/settings/templates/modals/aboutImageFormatsModal.html'      //<-- replaces inline version of ---->   template: '<p>Test Directive</p>'
        };
        return directive;

        function link(scope, element, attrs) {
        }
    }

})();