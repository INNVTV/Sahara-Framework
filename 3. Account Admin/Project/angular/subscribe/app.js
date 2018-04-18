(function () {
    'use strict';

    var id = 'app';

    // TODO: Inject modules as needed.
    var app = angular.module('app', [
        // Angular modules 
        //'ngAnimate',        // animations
        'ngRoute'            // routing

    ])
        .config(function ($routeProvider, $locationProvider) {

            $routeProvider.
                when("/sub/:planName/:frequencyMonths/:accountId", { templateUrl: "/angular/subscribe/partials/subscribe.html", caseInsensitiveMatch: true })

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