INSERT INTO PropertyType

(PropertyTypeID, PropertyTypeName, PropertyTypeNameKey, PropertyTypeDescription, OrderID)

SELECT					'71331f61-6e06-4e6a-b978-1ba25ea9e0b7',		'Predefined',		'predefined',		'A predefined list of values',						'0'
UNION ALL

SELECT					'f21a003f-6e58-43f5-b894-b594c91213d9',		'String',			'string',			'A short string value',								'1'
UNION ALL

SELECT					'86af4580-af33-4ab8-bf7f-b4925e5f40e9',		'Number',			'number',			'Any numeric or monetary value',					'2'
UNION ALL

SELECT					'18bcbaea-d901-4070-b68a-a97d75881c9f',		'Paragraph',		'paragraph',		'A paragraph of text',								'3'
UNION ALL

SELECT					'c86b8c6e-47b2-456a-ab14-8ef18b22596e',		'Date/Time',		'datetime',			'A date/time value',								'4'
UNION ALL

SELECT					'd1902817-7924-4de5-8b1d-3aa302d01ffb',		'Location',			'location',			'Address and map point',									'5'
UNION ALL

SELECT					'd98cd49c-507f-4c32-b287-3879872c0d83',		'Swatch',			'swatch',		    'Sample of color, texture or fabric',						'6'


--Merged with number
--SELECT				'3d84800e-e91b-4200-a3e0-cda5c5c67baa',		'Decimal',			'double',			'A decmal value',									'3'
--UNION ALL

--Merged with number
--SELECT				'98d4756d-4038-494a-b447-2c2ed018a96b',		'Money',			'money',			'A currency value',									'4'
--UNION ALL

--Meged with predefined
--SELECT				'f10307fe-4057-4486-a2d6-3741b79c0127',		'On/Off',			'bool',				'A value indicating on/off, yes/no or true/false',	'5'
--UNION ALL


--SELECT				'',											'Address',			'address',			'',													'6'
--UNION ALL

--SELECT				'',											'MapPoint',			'mappoint',			'A point on a map',									'6'
--UNION ALL

