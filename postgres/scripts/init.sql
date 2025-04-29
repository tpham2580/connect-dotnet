CREATE TABLE IF NOT EXISTS business (
	business_id SERIAL PRIMARY KEY,
	name VARCHAR(255) NOT NULL,
	address VARCHAR(255) NOT NULL,
	city VARCHAR (100) NOT NULL,
	state VARCHAR (50) NOT NULL,
	country VARCHAR (100) NOT NULL,
	latitude DOUBLE PRECISION,
	longitude DOUBLE PRECISION
);

INSERT INTO business (name, address, city, state, country, latitude, longitude) VALUES
('Saigon Vietnam Deli', '1200 S Jackson St # 7', 'Seattle', 'WA', 'USA', 47.59996796458999, -122.31665938469888),
('Mémoire Cà Phê', '1495 NE Alberta St', 'Portland', 'OR', 'USA', 45.55924712430121, -122.64987949582144);
