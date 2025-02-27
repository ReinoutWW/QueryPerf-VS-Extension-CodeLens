# CodeLens Query Performance - V2

A **Visual Studio 2022 Extension** that displays query performance statistics in the editor, leveraging **CodeLens**. The extension reads saved data (e.g., from a CSV file) to show **execution time**, **bytes sent/received**, **row counts**, **column counts**, and more—directly above each method in your source code.

<img src="https://github.com/ReinoutWW/QueryPerf-VS-Extension-CodeLens/blob/master/CodeLensQueryPerformance/QueryPerformance.png" alt="Example Image" width="300">

## Overview

- **Purpose**: Give developers immediate insight into performance metrics at the method level, without leaving the IDE.
- **Data Source**: A CSV file (for demo purposes) located at `C:\CodeLens\CodeLensSourceData3.csv`.
- **Identification**: Each row in the dataset corresponds to a specific “tag” (namespace + class + method signature).
- **Future**: An automatic connection to Azure Application Insights is planned for live, dynamic data.

## Preview

<img src="https://github.com/ReinoutWW/QueryPerf-VS-Extension-CodeLens/blob/master/PerfPreview.png" alt="Example Image" width="500">
*CodeLens Query Performance Extension Preview*

## How It Works

1. **Tag Detection**  
   The extension identifies methods by a “tag” (usually a combination of **namespace**, **class**, and **method**).  
   
2. **Data Lookup**  
   When you open or hover over a method in Visual Studio, CodeLens queries the CSV file for matching performance data (using the tag).

3. **Display Metrics**  
   If found, the extension shows performance metrics (min/max/avg/total) for items like:
   - Bytes Sent
   - Bytes Received
   - Total Bytes
   - Row Count
   - Column Count
   - Execution Time
   - (and more, as your dataset supports)

## Dataset Format

For this **demo** version, you must place your CSV dataset at: `C:\CodeLens\CodeLensSourceData3.csv`


Each row in the CSV should match the following schema:

| Column               | Description                                      |
|----------------------|--------------------------------------------------|
| Tag                  | Identifier (namespace.class.method)              |
| QueryCount           | Number of times the query was invoked            |
| UniqueUserCount      | Number of unique users who triggered the query   |
| BytesSent_Min        | Minimum bytes sent                               |
| BytesSent_Max        | Maximum bytes sent                               |
| BytesSent_Avg        | Average bytes sent                               |
| BytesSent_Total      | Total bytes sent                                 |
| BytesReceived_Min    | Minimum bytes received                           |
| BytesReceived_Max    | Maximum bytes received                           |
| BytesReceived_Avg    | Average bytes received                           |
| BytesReceived_Total  | Total bytes received                             |
| TotalBytes_Min       | Minimum total bytes (sent + received)           |
| TotalBytes_Max       | Maximum total bytes (sent + received)           |
| TotalBytes_Avg       | Average total bytes (sent + received)           |
| TotalBytes_Total     | Summation of total bytes (sent + received)       |
| Rows_Min             | Minimum rows returned (SelectRows + IduRows)     |
| Rows_Max             | Maximum rows returned                            |
| Rows_Avg             | Average rows returned                            |
| Rows_Total           | Total rows returned                              |
| Columns_Min          | Minimum column count                             |
| Columns_Max          | Maximum column count                             |
| Columns_Avg          | Average column count                             |
| Columns_Total        | Total columns across queries                     |
| ExecutionTime_Min    | Minimum execution time (milliseconds)            |
| ExecutionTime_Max    | Maximum execution time                           |
| ExecutionTime_Avg    | Average execution time                           |
| ExecutionTime_Total  | Summation of execution time across all queries   |

*(You can add or remove columns as needed, as long as the extension’s code is updated to parse them.)*

## Setup & Installation

1. **Build the Extension**  
   Open the solution in Visual Studio 2022 and build the *CodeLens Query Performance - V2* project.

2. **Install/Deploy**  
   - After building, you can install the resulting `.vsix` file or launch the experimental VS instance for testing.

3. **Prepare the CSV**  
   - Place your CSV at the path `C:\CodeLens\CodeLensSourceData3.csv`.
   - Ensure each row has a **tag** that matches your actual method signature (e.g., `MyProject.DataAccess.UserRepository.GetUserById`).

4. **Launch Visual Studio**  
   - Open any solution containing methods that match the tags in your CSV.
   - Verify CodeLens appears above those methods, showing the relevant performance data.

## Future Enhancements

- **Azure Application Insights Integration**  
  Instead of relying on a local CSV, the extension could pull real-time telemetry from Azure App Insights.
- **Query Interceptors**  
  Automate data collection by intercepting queries at runtime and storing metrics in a central repository.
- **Customizable Layout**  
  Let users choose which metrics to display or hide in CodeLens.

## Contributing

1. **Fork** this repo.
2. **Create** a feature branch.
3. **Commit** your changes.
4. **Open** a Pull Request with a clear description of your contribution.

## License

This project is released as a **demo**. Check the accompanying license file (if any) for details.

---

**Happy coding!** For any inquiries or bug reports, please open an issue or contact the project maintainer.
