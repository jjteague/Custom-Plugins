
-- =============================================
-- Author:      DTS - Amit M (toresolveissue@gmail.com)
-- Create date: 2018-08-11
-- Description: All Givers dashboard to show the people who gave in the given time period with filter accountlist with taxable or not
-- 				Grouped By Currency Type
-- =============================================

CREATE PROCEDURE [dbo].[spDTS_Finance_Filter_GroupByCurrency]
     @StartDate varchar(50) = null
    , @EndDate varchar(50) = null
	, @AccountList nvarchar(Max) = NULL
	 , @IsTax nvarchar(10) = NULL
	 , @CurrencyType nvarchar(Max) = NULL
    WITH RECOMPILE
AS

BEGIN
Declare @SD datetime,@ED datetime, @IsTaxDeductable bit

if(@IsTax = 'Tax' OR @IsTax = '') -- sets Tax status when DD block is freshly loaded
BEGIN
	SET @IsTax = 1
END

if(@AccountList = 'AccountList' OR @AccountList = '') -- sets Account List status when DD block is freshly loaded
BEGIN
	SET @AccountList = NULL
END

if(@CurrencyType = 'CurrencyType' OR @CurrencyType = '') -- sets Currency Types status when DD block is freshly loaded
BEGIN
	SET @CurrencyType = NULL
END

SET @IsTaxDeductable = 1
if(@IsTax = 0)
BEGIN
	SET @IsTaxDeductable = 0
END

if(@StartDate = 'startDate') -- sets Start Date when DD block is freshly loaded
begin
set @SD = convert(date, GetDate())
END
ELSE
BEGIN
set @SD = convert(date, @StartDate)
END

if(@EndDate = 'endDate') -- sets End Date when DD block is freshly loaded
begin
set @ED = convert(date, GetDate()) 
END
ELSE
BEGIN
set @ED = convert(date, @EndDate)
END

SELECT  (Select Value FROM DefinedValue WHERE ID = [T].[CurrencyTypeValueId]) AS [CurrencyType]
    ,SUM([T].[Amount]) AS [Amount]
FROM (
    SELECT                 
        [p].[GivingId]
        ,[p].[NickName] AS [FirstName]
        ,[p].[LastName]
        ,[fb].[Name] AS [BatchName]
        ,[fb].[Id] AS [BatchId]
        ,[fa].[Name] AS AccountName
        ,[ftd].[Amount]
        ,[fpd].[CurrencyTypeValueId]
		, [ft].[TransactionDateTime]
		, [ftd].[TransactionId]
    FROM [FinancialTransaction] [ft] WITH (NOLOCK)
    Left JOIN [FinancialBatch] [fb] WITH (NOLOCK) ON [fb].[Id] = [ft].[BatchId]
    INNER JOIN [FinancialTransactionDetail] [ftd] WITH (NOLOCK)    ON [ftd].[TransactionId] = [ft].[Id]
    INNER JOIN [FinancialPaymentDetail] [fpd] WITH (NOLOCK)    ON [fpd].Id = [ft].[FinancialPaymentDetailId]
    INNER JOIN [FinancialAccount] [fa] WITH (NOLOCK) ON [fa].[Id] = [ftd].[AccountId] AND (@IsTax = 2 OR [fa].[IsTaxDeductible] = @IsTaxDeductable)
    INNER JOIN [PersonAlias] [pa] WITH (NOLOCK)    ON [pa].[Id] = [ft].[AuthorizedPersonAliasId]
    INNER JOIN [Person] [p] WITH (NOLOCK)    ON [p].[Id] = [pa].[PersonId]
    WHERE [ft].[TransactionDateTime] >= @SD AND [ft].[TransactionDateTime] < @ED +1  
    	And (@IsTax = 2 OR [fa].[IsTaxDeductible] = @IsTaxDeductable) 
    	And (@AccountList IS NULL OR CAST(fa.Id as nvarchar) in (SELECT * FROM fnStringList2IntegerTable(@AccountList)))
		And (@CurrencyType IS NULL OR CAST(fpd.CurrencyTypeValueId as int) in (SELECT * FROM fnStringList2IntegerTable(@CurrencyType)))
    ) AS [T]    
GROUP BY [T].[CurrencyTypeValueId]
    Order By Amount DESC

END
