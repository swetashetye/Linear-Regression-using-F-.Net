
#r "System.Transactions.dll"


#r "../packages/Accord.3.6.0/lib/net40/Accord.dll"
#r "../packages/Accord.Statistics.3.6.0/lib/net40/Accord.Statistics.dll"
#r "../packages/Accord.Math.3.6.0/lib/net40/Accord.Math.dll"
#r "../packages/FSharp.Data.2.3.3/lib/net40/FSharp.Data.dll"



open Accord
open Accord.Statistics
open Accord.Statistics.Models.Regression.Linear
open System
open System.Data.SqlClient


type ProductReview = {ProductID:int; TotalOrders:float; AvgReviews:float}
let reviews = ResizeArray<ProductReview>()
[<Literal>]
let connectionString = "data source=DESKTOP-QFG8KV0\SQLEXPRESS2014;initial catalog=AdventureWorks2014;user id=sa;password=Duffy@2945;"
[<Literal>]
let query = "Select
            A.ProductID, TotalOrders, AvgReviews
            From
            (Select
            ProductID,
            Sum(OrderQty) as TotalOrders
            from [Sales].[SalesOrderDetail] as SOD
            inner join [Sales].[SalesOrderHeader] as SOH
            on SOD.SalesOrderID = SOH.SalesOrderID
            inner join [Sales].[Customer] as C
            on SOH.CustomerID = C.CustomerID
            Where C.StoreID is not null
            Group By ProductID) as A
            Inner Join
            (Select
            ProductID,
            (Sum(Rating) + 0.0) / (Count(ProductID) + 0.0) as
            AvgReviews
            from [Production].[ProductReview] as PR
            Group By ProductID) as B
            on A.ProductID = B.ProductID"

let connection = new SqlConnection(connectionString)
let command = new SqlCommand(query,connection)
connection.Open()
let reader = command.ExecuteReader()
while reader.Read() do
    reviews.Add({ProductID=reader.GetInt32(0);TotalOrders=(float)(reader.GetInt32(1));AvgReviews=(float)(reader.GetDecimal(2))})


let x = reviews |> Seq.map (fun pr -> pr.AvgReviews) |> Seq.toArray
let y = reviews |> Seq.map (fun pr -> pr.TotalOrders) |> Seq.toArray

let ols = OrdinaryLeastSquares()
let regression =ols.Learn(x,y)
let sse = regression.Regress(x,y)
let mse = sse/float x.Length
let rmse = sqrt mse
let r2 = regression.CoefficientOfDetermination(x,y)





//let regression = SimpleLinearRegression()
//let sse = regression.Regress(x,y)
//let mse = sse/float x.Length
//let rmse = sqrt mse
//let r2 = regression.CoefficientOfDetermination(x,y)