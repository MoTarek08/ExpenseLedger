# ExpenseLedger
ExpenseLedger is a backend service for personal expense management, designed to help users track spending, organize their finances, manage budgets, and monitor long-term spending goals.
It provides a secure and scalable foundation for features such as expense tracking, recurring expenses, receipt uploads, budgeting, analytics, and financial insights.

The project is built with **ASP.NET Core** using **Clean Architecture**, emphasizing separation of concerns, maintainability, and production-ready software engineering practices. Business rules are isolated within the domain, application logic is organized into use cases, and infrastructure concerns such as persistence, object storage, and background processing remain fully decoupled from the core application.

Beyond implementing business features, the project focuses on building software that is reliable and easy to evolve. Consistent coding conventions, structured logging, centralized error handling, comprehensive validation, background processing, rate limiting, OpenAPI documentation, and automated testing are treated as first-class concerns rather than afterthoughts.

# Prequests:
  - Having a clone from the repo
  - Install docker and open it

# Executing program
  1- Create appsettings.development.json file
  
  2- Copy the content of appsettings.development.example.json and paste it into appsettings.development.json
  
  3- Configure your secrets
  
  4- Copy the content of .env.example and paste it into .env (in the root directory)

  5- Configure the secrets to be in sync with the appsettings.development.json
  
  6- From the root of the working directory, execute:
    docker compose up
  
  7- Navigate the the Host project:
    cd Server/src/Host

  8- Run the application:
  dotnet run --launch-profile https


# Note:
  - appsettings.development.json: used for configuring secrets in the development environment.
  - .env: contains the secrets required for running the sevices and spinning up containers correctly.
  - Https runs on: "https://localhost:7000"
  - Http runs on: "http://localhost:5216"
  - Swagger runs on: "https://localhost:7000/swagger/index.html"

# Help:
  - If you faced issues with spinning docker containers, try to remove all containers from you docker, or at least any related containers, like Postgres containers or Minio
