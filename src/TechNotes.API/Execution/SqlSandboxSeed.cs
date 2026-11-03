namespace TechNotes.API.Execution;

public static class SqlSandboxSeed
{
    public const string SqlServer = """
        CREATE TABLE departments (
            id INT PRIMARY KEY,
            name VARCHAR(60) NOT NULL
        );

        CREATE TABLE employees (
            id INT PRIMARY KEY,
            name VARCHAR(80) NOT NULL,
            department_id INT NULL,
            manager_id INT NULL,
            salary DECIMAL(12, 2) NOT NULL,
            hired_on DATE NOT NULL,
            is_active BIT NOT NULL,
            CONSTRAINT fk_employees_department FOREIGN KEY (department_id) REFERENCES departments(id),
            CONSTRAINT fk_employees_manager FOREIGN KEY (manager_id) REFERENCES employees(id)
        );

        CREATE TABLE customers (
            id INT PRIMARY KEY,
            name VARCHAR(80) NOT NULL,
            city VARCHAR(60) NOT NULL,
            created_at DATETIME2 NOT NULL
        );

        CREATE TABLE orders (
            id INT PRIMARY KEY,
            customer_id INT NOT NULL,
            order_date DATE NOT NULL,
            status VARCHAR(20) NOT NULL,
            total DECIMAL(12, 2) NOT NULL,
            CONSTRAINT fk_orders_customer FOREIGN KEY (customer_id) REFERENCES customers(id)
        );

        CREATE TABLE order_items (
            order_id INT NOT NULL,
            product_name VARCHAR(80) NOT NULL,
            quantity INT NOT NULL,
            unit_price DECIMAL(12, 2) NOT NULL,
            CONSTRAINT fk_items_order FOREIGN KEY (order_id) REFERENCES orders(id)
        );

        CREATE TABLE accounts (
            id INT PRIMARY KEY,
            owner VARCHAR(80) NOT NULL,
            balance DECIMAL(12, 2) NOT NULL
        );

        CREATE TABLE audit_log (
            id INT IDENTITY(1, 1) PRIMARY KEY,
            event_name VARCHAR(80) NOT NULL,
            created_at DATETIME2 NOT NULL
        );

        INSERT INTO departments (id, name) VALUES
            (1, 'Engineering'), (2, 'Sales'), (3, 'Support'), (4, 'Finance'), (5, 'Legal');

        INSERT INTO employees (id, name, department_id, manager_id, salary, hired_on, is_active) VALUES
            (1, 'Ada Lovelace', 1, NULL, 95000.00, '2020-01-15', 1),
            (2, 'Grace Hopper', 1, 1, 105000.00, '2019-03-01', 1),
            (3, 'Alan Turing', 1, 1, 98000.00, '2021-07-12', 1),
            (4, 'Katherine Johnson', 2, NULL, 87000.00, '2018-11-05', 1),
            (5, 'Linus Torvalds', 1, 1, 92000.00, '2022-02-20', 1),
            (6, 'Margaret Hamilton', 3, 2, 89000.00, '2020-09-30', 1),
            (7, 'Barbara Liskov', 3, 2, 91000.00, '2017-06-18', 1),
            (8, 'Dennis Ritchie', 2, 4, 85000.00, '2023-01-09', 1),
            (9, 'Ken Thompson', 2, 4, 88000.00, '2021-04-11', 1),
            (10, 'Radia Perlman', 4, NULL, 99000.00, '2016-08-22', 1),
            (11, 'James Gosling', 4, 10, 84000.00, '2022-10-03', 0);

        INSERT INTO customers (id, name, city, created_at) VALUES
            (1, 'Acme Corp', 'London', '2023-01-10T09:00:00'),
            (2, 'Beta Labs', 'Berlin', '2023-02-14T10:30:00'),
            (3, 'Gamma Works', 'Paris', '2023-03-20T14:15:00'),
            (4, 'Delta Systems', 'London', '2023-04-05T08:45:00'),
            (5, 'Epsilon Group', 'Madrid', '2023-05-18T16:20:00');

        INSERT INTO orders (id, customer_id, order_date, status, total) VALUES
            (1, 1, '2024-01-10', 'completed', 1200.00),
            (2, 1, '2024-02-15', 'pending', 450.00),
            (3, 2, '2024-01-20', 'completed', 990.00),
            (4, 3, '2024-03-01', 'cancelled', 300.00),
            (5, 2, '2024-03-12', 'completed', 1500.00),
            (6, 4, '2024-04-05', 'pending', 750.00);

        INSERT INTO order_items (order_id, product_name, quantity, unit_price) VALUES
            (1, 'Widget', 2, 300.00),
            (1, 'Gadget', 1, 600.00),
            (2, 'Widget', 1, 450.00),
            (3, 'Service', 1, 990.00),
            (4, 'Widget', 1, 300.00),
            (5, 'Gadget', 1, 1500.00),
            (6, 'Service', 1, 750.00);

        INSERT INTO accounts (id, owner, balance) VALUES
            (1, 'Ada Lovelace', 5000.00),
            (2, 'Grace Hopper', 7500.00),
            (3, 'Alan Turing', 3200.00);
        """;

    public const string Postgres = """
        CREATE TABLE departments (
            id INTEGER PRIMARY KEY,
            name VARCHAR(60) NOT NULL
        );

        CREATE TABLE employees (
            id INTEGER PRIMARY KEY,
            name VARCHAR(80) NOT NULL,
            department_id INTEGER NULL,
            manager_id INTEGER NULL,
            salary NUMERIC(12, 2) NOT NULL,
            hired_on DATE NOT NULL,
            is_active BOOLEAN NOT NULL,
            CONSTRAINT fk_employees_department FOREIGN KEY (department_id) REFERENCES departments(id),
            CONSTRAINT fk_employees_manager FOREIGN KEY (manager_id) REFERENCES employees(id)
        );

        CREATE TABLE customers (
            id INTEGER PRIMARY KEY,
            name VARCHAR(80) NOT NULL,
            city VARCHAR(60) NOT NULL,
            created_at TIMESTAMP NOT NULL
        );

        CREATE TABLE orders (
            id INTEGER PRIMARY KEY,
            customer_id INTEGER NOT NULL,
            order_date DATE NOT NULL,
            status VARCHAR(20) NOT NULL,
            total NUMERIC(12, 2) NOT NULL,
            CONSTRAINT fk_orders_customer FOREIGN KEY (customer_id) REFERENCES customers(id)
        );

        CREATE TABLE order_items (
            order_id INTEGER NOT NULL,
            product_name VARCHAR(80) NOT NULL,
            quantity INTEGER NOT NULL,
            unit_price NUMERIC(12, 2) NOT NULL,
            CONSTRAINT fk_items_order FOREIGN KEY (order_id) REFERENCES orders(id)
        );

        CREATE TABLE accounts (
            id INTEGER PRIMARY KEY,
            owner VARCHAR(80) NOT NULL,
            balance NUMERIC(12, 2) NOT NULL
        );

        CREATE TABLE audit_log (
            id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
            event_name VARCHAR(80) NOT NULL,
            created_at TIMESTAMP NOT NULL
        );

        INSERT INTO departments (id, name) VALUES
            (1, 'Engineering'), (2, 'Sales'), (3, 'Support'), (4, 'Finance'), (5, 'Legal');

        INSERT INTO employees (id, name, department_id, manager_id, salary, hired_on, is_active) VALUES
            (1, 'Ada Lovelace', 1, NULL, 95000.00, '2020-01-15', TRUE),
            (2, 'Grace Hopper', 1, 1, 105000.00, '2019-03-01', TRUE),
            (3, 'Alan Turing', 1, 1, 98000.00, '2021-07-12', TRUE),
            (4, 'Katherine Johnson', 2, NULL, 87000.00, '2018-11-05', TRUE),
            (5, 'Linus Torvalds', 1, 1, 92000.00, '2022-02-20', TRUE),
            (6, 'Margaret Hamilton', 3, 2, 89000.00, '2020-09-30', TRUE),
            (7, 'Barbara Liskov', 3, 2, 91000.00, '2017-06-18', TRUE),
            (8, 'Dennis Ritchie', 2, 4, 85000.00, '2023-01-09', TRUE),
            (9, 'Ken Thompson', 2, 4, 88000.00, '2021-04-11', TRUE),
            (10, 'Radia Perlman', 4, NULL, 99000.00, '2016-08-22', TRUE),
            (11, 'James Gosling', 4, 10, 84000.00, '2022-10-03', FALSE);

        INSERT INTO customers (id, name, city, created_at) VALUES
            (1, 'Acme Corp', 'London', '2023-01-10 09:00:00'),
            (2, 'Beta Labs', 'Berlin', '2023-02-14 10:30:00'),
            (3, 'Gamma Works', 'Paris', '2023-03-20 14:15:00'),
            (4, 'Delta Systems', 'London', '2023-04-05 08:45:00'),
            (5, 'Epsilon Group', 'Madrid', '2023-05-18 16:20:00');

        INSERT INTO orders (id, customer_id, order_date, status, total) VALUES
            (1, 1, '2024-01-10', 'completed', 1200.00),
            (2, 1, '2024-02-15', 'pending', 450.00),
            (3, 2, '2024-01-20', 'completed', 990.00),
            (4, 3, '2024-03-01', 'cancelled', 300.00),
            (5, 2, '2024-03-12', 'completed', 1500.00),
            (6, 4, '2024-04-05', 'pending', 750.00);

        INSERT INTO order_items (order_id, product_name, quantity, unit_price) VALUES
            (1, 'Widget', 2, 300.00),
            (1, 'Gadget', 1, 600.00),
            (2, 'Widget', 1, 450.00),
            (3, 'Service', 1, 990.00),
            (4, 'Widget', 1, 300.00),
            (5, 'Gadget', 1, 1500.00),
            (6, 'Service', 1, 750.00);

        INSERT INTO accounts (id, owner, balance) VALUES
            (1, 'Ada Lovelace', 5000.00),
            (2, 'Grace Hopper', 7500.00),
            (3, 'Alan Turing', 3200.00);
        """;
}
