﻿(function () {
    'use strict';

    // TODO: replace app with your module name
    angular.module('app').directive('accountBillingPaymentHistoryPanel', ['$window', accountBillingPaymentHistoryPanel]);

    function accountBillingPaymentHistoryPanel($window) {
        // Usage:
        // 
        // Creates:
        // 
        var directive = {
            link: link,
            replace: true,     //<-- Replace the directive element with the template completly
            restrict: 'AEC',    //<-- Allows use as: Attribute, Element and Class
            templateUrl: '/angular/account/templates/accountBillingPaymentHistoryPanel.html'   //<-- replaces inline version of ---->   template: '<p>Test Directive</p>'
            //controller: controller
        }
        return directive;

        function link(scope, element, attrs) {

            
        }

        

    }

})();