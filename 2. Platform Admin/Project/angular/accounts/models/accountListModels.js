(function () {
    'use strict';

    var serviceId = 'accountListModels';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, [accountListModels]);

    function accountListModels() {
        // Define the functions and properties to reveal.
        var service = {
            getFilters: getFilters,
            getSortBy: getSortBy,
            getSortDirection: getSortDirection,
            getAmountsPerPage: getAmountsPerPage,
            //getPageState: getPageState,
        };

        return service;

        function getFilters() {

            return [
                { label: 'All Accounts',            name: '*',                                  value: '*' },
                { label: 'Active Accounts',         name: 'Active',                             value: '1' },
                { label: 'Inactive Accounts',       name: 'Active',                             value: '0' },

                { label: 'hr', name: '', value: '' },
                //{ label: 'Trial Accounts',          name: 'PaymentPlans.PaymentPlanName',       value: 'Trial' },
                //{ label: 'Free Accounts',           name: 'PaymentPlans.PaymentPlanName', value: 'Free' },
                //Make these dynamic
                //{ label: 'Small Accounts',          name: 'PaymentPlans.PaymentPlanName',       value: 'Small' },
                //{ label: 'Medium Accounts',         name: 'PaymentPlans.PaymentPlanName',       value: 'Medium' },
                { label: 'PastDue/Unpaid', name: 'Delinquent', value: '1' },
                { label: 'Unprovisioned Accounts', name: 'Provisioned', value: '0' },
                { label: 'hr', name: '', value: '' },
                //{ label: 'Scheduled For Closure',  name: 'AccountEndDate',    value: 'IS NOT NULL' }
            ];
        }

        function getSortBy() {

            return [
                { label: 'Created Date', value: 'CreatedDate' },
                { label: 'Account Name', value: 'AccountName' }
            ];

        }

        function getSortDirection() {

            return [
                { label: 'Desc', value: 'Desc' },
                { label: 'Asc', value: 'Asc' }
            ];
        }

        function getAmountsPerPage() {

            return [
                /*Values used for platform testing
                { label: '3', value: 3 },
                { label: '4', value: 4 },*/
                { label: '20', value: 20 }, //<-- serves as the default amount per page
                { label: '50',  value: 50 },
                { label: '100', value: 100 },
                /*{ label: '200', value: 200 }*/
            ];
        }



        ////--------------------
        /*
        function getPageState() {

            var pageState = {

                currentPage             : 0,
                totalPages              : 0,
                totalAccounts           : 0,
                pageMode                : 0,
                currentFilter           : null,
                currentSortBy           : null,
                currentSortDirection    : null,
                currentAmountPerPage  : null,
            }

            return pageState;

        }
        */
    }

})();