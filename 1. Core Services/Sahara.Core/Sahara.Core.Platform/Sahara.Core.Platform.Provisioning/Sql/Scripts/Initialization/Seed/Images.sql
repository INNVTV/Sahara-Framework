INSERT INTO #schema#.ImageGroup

(ImageGroupTypeNameKey, ImageGroupID, ImageGroupName, ImageGroupNameKey, AllowDeletion)

SELECT					'account',				'0A980214-8B21-492F-8476-E7075D94AA85',			'Logos',			'logos',	'0'	
UNION ALL

SELECT					'product',				'01539876-8eb6-4098-902f-06992805e7a9',			'Default',			'default',	'0'		
UNION ALL

SELECT					'product',				'8180f207-84b6-4ec7-a31a-d94931109f3c',			'Main',				'main',		'0'		
UNION ALL

SELECT					'category',				'c41eabb9-07ec-4449-b43a-001a8326e1d6',			'Default',			'default',	'0'		
UNION ALL

SELECT					'subcategory',			'e50a8d55-b1e0-48bb-bf93-2ef5ec26b3f0',			'Default',			'default',	'0'	
UNION ALL

SELECT					'subsubcategory',		'46c2cda4-df0e-4caf-a8a2-bf8c5b7789c2',			'Default',			'default',	'0'		
UNION ALL

SELECT					'subsubsubcategory',	'de4667ef-8bdc-4c51-9752-350dcc16eac9',			'Default',			'default',	'0'	
Go


INSERT INTO #schema#.ImageFormat

(ImageFormatID, ImageGroupTypeNameKey, ImageGroupNameKey, ImageFormatName, ImageFormatNameKey, Width, Height, Gallery, Listing, AllowDeletion)

SELECT					'F229D43E-0D55-482B-BAB0-E8D541AC7EC4',			'account',				'logos',		'Square',			'square',		'600', '600', '0', '0', '0'
UNION ALL
	
SELECT					'3C716C00-BA76-4CE1-B3F9-603B5E7697D8',			'account',				'logos',		'Wide',				'wide',			'1200', '600', '0', '0', '0'
UNION ALL	

SELECT					'3c9c8332-b44b-4634-a77b-b157e4b459a0',			'product',				'default',		'Thumbnail',		'thumbnail',	'600', '600', '0', '1', '0'
UNION ALL	

SELECT					'81338b6e-9ac7-4da7-9ac2-3064f9a86891',			'product',				'main',			'Featured',			'featured',		'850', '1200', '0', '0', '0'
UNION ALL	

SELECT					'601e4227-29db-4252-b4c9-faa09af8f39b',			'product',				'main',			'Gallery',			'gallery',		'0',  '1200', '1', '0', '0'
UNION ALL																																				 
																																						 
SELECT					'22125ad7-71dd-42ea-8181-725b5e4bef72',			'category',				'default',		'Thumbnail',		'thumbnail',	'600', '600', '0', '1', '0'
UNION ALL																																				 
																																						 
SELECT					'0fec44ef-9169-4fe3-8801-0d4437ed5217',			'subcategory',			'default',		'Thumbnail',		'thumbnail',	'600', '600', '0', '1', '0'
UNION ALL																																				 
																																						 
SELECT					'cd806d8a-958e-43f5-aafc-b3d07843f86b',			'subsubcategory',		'default',		'Thumbnail',		'thumbnail',	'600', '600', '0', '1', '0'
UNION ALL																																				 
																																						 
SELECT					'a433bc48-297b-4012-8283-22c076c28a4e',			'subsubsubcategory',	'default',		'Thumbnail',		'thumbnail',	'600', '600', '0', '1', '0'
Go
