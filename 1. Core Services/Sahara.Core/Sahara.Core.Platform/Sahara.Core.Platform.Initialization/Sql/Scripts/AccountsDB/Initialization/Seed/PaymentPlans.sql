INSERT INTO PaymentPlans

(PaymentPlanName, MonthlyRate, MaxUsers,
MaxCategorizationsPerSet, MaxProductsPerSet, MaxProducts,
MaxProperties, MaxValuesPerProperty, MaxTags,
AllowThemes, AllowSalesLeads, AllowImageEnhancements, AllowLocationData, AllowCustomOrdering, MonthlySupportHours, Visible,
MaxImageGroups, MaxImageFormats, MaxImageGalleries, MaxImagesPerGallery, SearchPlan, MonthlyStorage, MonthlyCDN, MonthlyTransactional)

-- "Name" is used as the plan ID along with the FrequencyID. This cannot be changed or duplicated due to STRIPE dependancies


-- ===========================================================================
--           The UNPROVISIONED plan is what ALL accounts are assigned to when they register
-- ===========================================================================

SELECT		'Unprovisioned',
			/* rate*/ '0',	/*users*/ '0',
			/*categorizationsperset*/ '0',/* productsPerSet*/ '0', /* products*/ '0',
            /* properties*/ '0', /* valuesPerProperty*/ '0', /* tags*/ '0', 
		    /* themes*/ '0', /* leads*/ '0', /* enhancements*/ '0', 
			/* location data*/ '0', /* custom ordering*/ '0', /* support hours*/ '0',
			/* visible*/ '0',	/* imageGroups*/ '0', /* imageFormats*/ '0',
			/* galleries*/ '0', /* maxImagesPergallery*/ '0', /* searchPlan*/ '',
			/* storage*/ '0', /* cdn*/ '0', /* transactional*/ '0'

UNION ALL


-- ===========================================================================
--           Plans used for TESTING
-- ===========================================================================

-------------------------------------------- TEST PLANS FOR FREE SEARCH TIER ---------------------------------------------

SELECT		'Test-Free-3-Small', /* Can only have 3 test accounts at a time. Limits: 432 products  / 10 search properties  */
			/* rate*/ '.99',	/*users*/ '2',
			/*categorizationsperset*/ '3',	/* productsPerSet*/ '15', /* products*/ '50',
            /* properties*/ '3', /* valuesPerProperty*/ '2', /* tags*/ '4',
		    /* themes*/ '1', /* leads*/ '1', /* enhancements*/ '1', 
			/* location data*/ '0', /* custom ordering*/ '1', /* support hours*/ '1',
			/* visible*/ '0',	/* imageGroups*/ '0', /* imageFormats*/ '0',
			/* galleries*/ '2', /* maxImagesPergallery*/ '6', /* searchPlan*/ 'Free-3', /* <-- Number after dash denotes shared tenant count on search service */
			/* storage*/ '100000', /* cdn*/ '20000', /* transactional*/ '20000'
UNION ALL

SELECT		'Test-Free-3-Medium', /* Can only have 3 test accounts at a time. Limits: 432 products  / 10 search properties  */
			/* rate*/ '1.29',	/*users*/ '3',
			/*categorizationsperset*/ '6',	/* productsPerSet*/ '200', /* products*/ '200',
            /* properties*/ '8', /* valuesPerProperty*/ '4', /* tags*/ '12',
		    /* themes*/ '1', /* leads*/ '1', /* enhancements*/ '1', 
			/* location data*/ '1', /* custom ordering*/ '1', /* support hours*/ '2',
			/* visible*/ '0',	/* imageGroups*/ '2', /* imageFormats*/ '4',
			/* galleries*/ '4', /* maxImagesPergallery*/ '8', /* searchPlan*/ 'Free-3', /* <-- Number after dash denotes shared tenant count on search service */
			/* storage*/ '100000', /* cdn*/ '20000', /* transactional*/ '20000'
UNION ALL

SELECT		'Test-Free-3-Large', /* Can only have 3 test accounts at a time. Limits: 432 products  / 10 search properties  */
			/* rate*/ '2.00',	/*users*/ '4',
			/*categorizationsperset*/ '25',	/* productsPerSet*/ '500', /* products*/ '3333',
            /* properties*/ '200', /* valuesPerProperty*/ '6', /* tags*/ '300',
		    /* themes*/ '1', /* leads*/ '1', /* enhancements*/ '1', 
			/* location data*/ '1', /* custom ordering*/ '1', /* support hours*/ '4',
			/* visible*/ '0',	/* imageGroups*/ '4', /* imageFormats*/ '6',
			/* galleries*/ '6', /* maxImagesPergallery*/ '90', /* searchPlan*/ 'Free-3', /* <-- Number after dash denotes shared tenant count on search service */
			/* storage*/ '100000', /* cdn*/ '20000', /* transactional*/ '20000'
UNION ALL

-------------------------------------------- TEST PLANS FOR BASIC SEARCH TIER ---------------------------------------------

SELECT		'Test-Basic-6-Small', /* A New Test Plan Allowing Us to Use BASIC Search Tier*/
			/* rate*/ '2.05',	/*users*/ '2',
			/*categorizationsperset*/ '3',	/* productsPerSet*/ '15', /* products*/ '50',
            /* properties*/ '3', /* valuesPerProperty*/ '2', /* tags*/ '4',
		    /* themes*/ '1', /* leads*/ '1', /* enhancements*/ '1', 
			/* location data*/ '0', /* custom ordering*/ '1', /* support hours*/ '1',
			/* visible*/ '0',	/* imageGroups*/ '0', /* imageFormats*/ '0',
			/* galleries*/ '2', /* maxImagesPergallery*/ '6', /* searchPlan*/ 'Basic-6', /* <-- Number after dash denotes shared tenant count on search service */
			/* storage*/ '100000', /* cdn*/ '20000', /* transactional*/ '20000'
UNION ALL

SELECT		'Test-Basic-6-Large', /* A New Test Plan Allowing Us to Use BASIC Search Tier*/
			/* rate*/ '3.20',	/*users*/ '4',
			/*categorizationsperset*/ '25',	/* productsPerSet*/ '500', /* products*/ '15000',
            /* properties*/ '200', /* valuesPerProperty*/ '6', /* tags*/ '300',
		    /* themes*/ '1', /* leads*/ '1', /* enhancements*/ '1', 
			/* location data*/ '1', /* custom ordering*/ '1', /* support hours*/ '4',
			/* visible*/ '0',	/* imageGroups*/ '4', /* imageFormats*/ '6',
			/* galleries*/ '6', /* maxImagesPergallery*/ '90', /* searchPlan*/ 'Basic-6', /* <-- Number after dash denotes shared tenant count on search service */
			/* storage*/ '100000', /* cdn*/ '20000', /* transactional*/ '20000'
UNION ALL

-- ===========================================================================
--           Short term TRIAL plans
-- ===========================================================================



SELECT		'Trial-Week', /* Can only have 3 test accounts at a time. Limits: 432 products  / 10 search properties  */
			/* rate*/ '5.99',	/*users*/ '3',
			/*categorizationsperset*/ '6',	/* productsPerSet*/ '100', /* products*/ '500',
            /* properties*/ '10', /* valuesPerProperty*/ '6', /* tags*/ '12',
		    /* themes*/ '1', /* leads*/ '1', /* enhancements*/ '1', 
			/* location data*/ '1', /* custom ordering*/ '1', /* support hours*/ '0',
			/* visible*/ '0',	/* imageGroups*/ '2', /* imageFormats*/ '4',
			/* galleries*/ '2', /* maxImagesPergallery*/ '20', /* searchPlan*/ 'Free-3', /* <-- Number after dash denotes shared tenant count on search service */
			/* storage*/ '100000', /* cdn*/ '20000', /* transactional*/ '20000'
UNION ALL

SELECT		'Trial-Month', /* Can only have 3 test accounts at a time. Limits: 432 products  / 10 search properties  */
			/* rate*/ '29.99',	/*users*/ '3',
			/*categorizationsperset*/ '6',	/* productsPerSet*/ '100', /* products*/ '500',
            /* properties*/ '10', /* valuesPerProperty*/ '6', /* tags*/ '12',
		    /* themes*/ '1', /* leads*/ '1', /* enhancements*/ '1', 
			/* location data*/ '1', /* custom ordering*/ '1', /* support hours*/ '0',
			/* visible*/ '0',	/* imageGroups*/ '2', /* imageFormats*/ '4',
			/* galleries*/ '2', /* maxImagesPergallery*/ '20', /* searchPlan*/ 'Free-3', /* <-- Number after dash denotes shared tenant count on search service */
			/* storage*/ '100000', /* cdn*/ '20000', /* transactional*/ '20000'
UNION ALL



--========================================================================

--BASED ON ALL TESTS WE ARE SAFE TO OFFER MAX 500,000 documents on any plan going into a single 10gb DocumentDB collection at a size of 18-20k per document with 60-80 properties.
-- Most test documents were coming in at 16.2-17k even with 72 dense properties so we should be very safe.

-- ===========================================================================
--           PLANS (BASIC Tier | 4 Tenants Shared)
-- ===========================================================================


-- Basic search plan allows for:
------------------------------------
--     -   5 indexes
--     -   1m documents.
--     -   2gb of storage

------------------------------------
-- 4 tenants at full capacity will fill:
------------------------------------
--     -   4/5 indexes
--     -   60k/1m documents.
--     -   1.8gb/2gb of storage (@ 30k per document * 60k documents)
------------------------------------

SELECT		'Starter', 
			/* rate*/ '249.00',	/*users*/ '3',
			/* categorizationsperset*/ '8',	/* productsPerSet*/ '120', /* products*/ '5000',
            /* properties*/ '10', /* valuesPerProperty*/ '10', /* tags*/ '20',
		    /* themes*/ '1', /* leads*/ '0', /* enhancements*/ '1', 
			/* location data*/ '0', /* custom ordering*/ '1', /* support hours*/ '1',
			/* visible*/ '1',	/* imageGroups*/ '1', /* imageFormats*/ '1',
			/* galleries*/ '1', /* maxImagesPergallery*/ '10', /* searchPlan*/ 'Basic-4', /* <-- Number after dash denotes shared tenant count on search service */
			/* storage*/ '100000', /* cdn*/ '10000', /* transactional*/ '10000'
UNION ALL

SELECT		'Micro', 
			/* rate*/ '359.00',	/*users*/ '4',
			/* categorizationsperset*/ '12',	/* productsPerSet*/ '140', /* products*/ '10000',
            /* properties*/ '12', /* valuesPerProperty*/ '12', /* tags*/ '30',
		    /* themes*/ '1', /* leads*/ '1', /* enhancements*/ '1', 
			/* location data*/ '0', /* custom ordering*/ '1', /* support hours*/ '1',
			/* visible*/ '0',	/* imageGroups*/ '1', /* imageFormats*/ '2',
			/* galleries*/ '1', /* maxImagesPergallery*/ '12', /* searchPlan*/ 'Basic-4', /* <-- Number after dash denotes shared tenant count on search service */
			/* storage*/ '100000', /* cdn*/ '10000', /* transactional*/ '10000'
UNION ALL


SELECT		'Basic', 
			/* rate*/ '699.00',	/*users*/ '6',
			/* categorizationsperset*/ '16',	/* productsPerSet*/ '200', /* products*/ '20000',
            /* properties*/ '20', /* valuesPerProperty*/ '20', /* tags*/ '40',
		    /* themes*/ '1', /* leads*/ '1', /* enhancements*/ '1', 
			/* location data*/ '0', /* custom ordering*/ '1', /* support hours*/ '2',
			/* visible*/ '1',	/* imageGroups*/ '1', /* imageFormats*/ '3',
			/* galleries*/ '2', /* maxImagesPergallery*/ '20', /* searchPlan*/ 'Basic-4', /* <-- Number after dash denotes shared tenant count on search service */
			/* storage*/ '500000', /* cdn*/ '20000', /* transactional*/ '20000'
UNION ALL

SELECT		'Plus', 
			/* rate*/ '899.00',	/*users*/ '6',
			/* categorizationsperset*/ '25',	/* productsPerSet*/ '250', /* products*/ '20000',
            /* properties*/ '25', /* valuesPerProperty*/ '25', /* tags*/ '60',
		    /* themes*/ '1', /* leads*/ '1', /* enhancements*/ '1', 
			/* location data*/ '0', /* custom ordering*/ '1', /* support hours*/ '2',
			/* visible*/ '0',	/* imageGroups*/ '2', /* imageFormats*/ '4',
			/* galleries*/ '2', /* maxImagesPergallery*/ '20', /* searchPlan*/ 'Basic-4', /* <-- Number after dash denotes shared tenant count on search service */
			/* storage*/ '800000', /* cdn*/ '30000', /* transactional*/ '30000'
UNION ALL

-- ===========================================================================
--           PLANS (STANDARD Tier | 2 Tenants Shared)
-- ===========================================================================

-- Standard search plan allows for:
-----------------------------------------------
--     -   50 indexes
--     -   15m documents.
--     -   25gb of storage

-------------------------------------------------
-- 2 tenants at full capacity will fill:
------------------------------------------------
--     -   2/50 indexes
--     -   1m/15m documents.
--     -   25gb/25gb of storage (@ 25k per document * 1m documents)
--                     -- [12.5gb in each respective documentDB partition]

------------ More Realitically------------------

--     -   20gb/25gb of storage (@ 20k per document * 1m documents)
--                     -- [10gb in each respective documentDB partition]

------------------------------------

SELECT		'Standard',
			/* rate*/ '1299.00',	/*users*/ '8',
			/* categorizationsperset*/ '45',	/* productsPerSet*/ '300', /* products*/ '75000',
            /* properties*/ '30', /* valuesPerProperty*/ '30', /* tags*/ '100',
		    /* themes*/ '1', /* leads*/ '1', /* enhancements*/ '1', 
			/* location data*/ '1', /* custom ordering*/ '1', /* support hours*/ '4',
			/* visible*/ '0',	/* imageGroups*/ '3', /* imageFormats*/ '6',
			/* galleries*/ '3', /* maxImagesPergallery*/ '30', /* searchPlan*/ 'Standard-2', /* <-- Number after dash denotes shared tenant count on search service */
			/* storage*/ '800000', /* cdn*/ '40000', /* transactional*/ '40000'
UNION ALL

SELECT		'Complete',
			/* rate*/ '1499.00',	/*users*/ '10',
			/* categorizationsperset*/ '50',	/* productsPerSet*/ '400', /* products*/ '100000',
            /* properties*/ '40', /* valuesPerProperty*/ '40', /* tags*/ '150',
		    /* themes*/ '1', /* leads*/ '1', /* enhancements*/ '1', 
			/* location data*/ '1', /* custom ordering*/ '1', /* support hours*/ '4',
			/* visible*/ '1',	/* imageGroups*/ '6', /* imageFormats*/ '10',
			/* galleries*/ '4', /* maxImagesPergallery*/ '40', /* searchPlan*/ 'Standard-2', /* <-- Number after dash denotes shared tenant count on search service */
			/* storage*/ '1000000', /* cdn*/ '50000', /* transactional*/ '50000'
UNION ALL

SELECT		'Professional',
			/* rate*/ '2699.00',	/*users*/ '20',
			/* categorizationsperset*/ '60',	/* productsPerSet*/ '500', /* products*/ '250000',
            /* properties*/ '60', /* valuesPerProperty*/ '50', /* tags*/ '200',
		    /* themes*/ '1', /* leads*/ '1', /* enhancements*/ '1', 
			/* location data*/ '1', /* custom ordering*/ '1', /* support hours*/ '8',
			/* visible*/ '1',	/* imageGroups*/ '8', /* imageFormats*/ '15',
			/* galleries*/ '6', /* maxImagesPergallery*/ '50', /* searchPlan*/ 'Standard-2', /* <-- Number after dash denotes shared tenant count on search service */
			/* storage*/ '2000000', /* cdn*/ '1000000', /* transactional*/ '100000'
UNION ALL

SELECT		'Team',
			/* rate*/ '3499.00',	/*users*/ '30',
			/* categorizationsperset*/ '75',	/* productsPerSet*/ '600', /* products*/ '300000',
            /* properties*/ '70', /* valuesPerProperty*/ '60', /* tags*/ '250',
		    /* themes*/ '1', /* leads*/ '1', /* enhancements*/ '1', 
			/* location data*/ '1', /* custom ordering*/ '1', /* support hours*/ '12',
			/* visible*/ '0',	/* imageGroups*/ '10', /* imageFormats*/ '15',
			/* galleries*/ '6', /* maxImagesPergallery*/ '60', /* searchPlan*/ 'Standard-2', /* <-- Number after dash denotes shared tenant count on search service */
			/* storage*/ '3000000', /* cdn*/ '1000000', /* transactional*/ '100000'
UNION ALL


SELECT		'Business',
			/* rate*/ '5899.00',	/*users*/ '40',
			/* categorizationsperset*/ '80',	/* productsPerSet*/ '800', /* products*/ '500000',
            /* properties*/ '80', /* valuesPerProperty*/ '60', /* tags*/ '300',
		    /* themes*/ '1', /* leads*/ '1', /* enhancements*/ '1', 
			/* location data*/ '1', /* custom ordering*/ '1', /* support hours*/ '24',
			/* visible*/ '1',	/* imageGroups*/ '10', /* imageFormats*/ '20',
			/* galleries*/ '8', /* maxImagesPergallery*/ '60', /* searchPlan*/ 'Standard-2', /* <-- Number after dash denotes shared tenant count on search service */
			/* storage*/ '5000000', /* cdn*/ '2000000', /* transactional*/ '200000'

-- ===========================================================================
--           PLANS (STANDARD2 Tier | 2 Tenants Shared) MERGED WITH ABOVE
-- ===========================================================================

-- S1 search plan allows for:
------------------------------------
--     -   200 indexes
--     -   60m documents.
--     -   100gb of storage

------------------------------------
-- 2 tenants at full capacity will fill:
------------------------------------
--     -   2/200 indexes
--     -   1m/60m documents.
--     -   30gb/100gb of storage (@ 30k per document * 1m documents)
--              - [15gb in each respective documentDB partition - NEED TO GET LARGER STORAGE OPTION TO ACCOUNT FOR THIS!!!!]
--              - [IF docs stay at 20k or under we can fit perfectly into 10gb DocDB accounts (20k * 500,000 docs)
------------------------------------
--
--SELECT		'Business',
			--/* rate*/ '5899.00',	/*users*/ '40',
			--/* categorizationsperset*/ '80',	/* productsPerSet*/ '800', /* products*/ '500000',
            --/* properties*/ '80', /* valuesPerProperty*/ '60', /* tags*/ '400',
		    --/* themes*/ '1', /* leads*/ '1', /* enhancements*/ '1', 
			--/* location data*/ '1', /* custom ordering*/ '1', /* support hours*/ '24',
			--/* visible*/ '1',	/* imageGroups*/ '8', /* imageFormats*/ '16',
			--/* galleries*/ '8', /* maxImagesPergallery*/ '60', /* searchPlan*/ 'S2-2' /* <-- Number after dash denotes shared tenant count on search service */
			




