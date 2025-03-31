# Remarks

Docker engine must be running to execute the E2E tests.

I chose not to use an in-memory database for the E2E tests, despite the fact that the tests aren't representing the application's current state.

There could have been a seperate project for the test data builders and the Web.E2ETests project could have implemented BuildAsync as extension methods.

There is one modification in the src folder: [assembly: InternalsVisibleTo("Betsson.OnlineWallets.Web.E2ETests")] was added to the OnlineWalletContext class.

# qa-backend-code-challenge

Code challenge for QA Backend Engineer candidates.

### Build Docker image

Run this command from the directory where there is the solution file.

```
docker build -f src/Betsson.OnlineWallets.Web/Dockerfile .
```

### Run Docker container

```
docker run -p <port>:8080 <image id>
```

### Open Swagger

```
http://localhost:<port>/swagger/index.html
```
