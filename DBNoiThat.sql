USE [master]
GO
CREATE DATABASE [DBNoiThat]
 CONTAINMENT = NONE
 ON  PRIMARY 
GO
ALTER DATABASE [DBNoiThat] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [DBNoiThat] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [DBNoiThat] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [DBNoiThat] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [DBNoiThat] SET ARITHABORT OFF 
GO
ALTER DATABASE [DBNoiThat] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [DBNoiThat] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [DBNoiThat] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [DBNoiThat] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [DBNoiThat] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [DBNoiThat] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [DBNoiThat] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [DBNoiThat] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [DBNoiThat] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [DBNoiThat] SET  DISABLE_BROKER 
GO
ALTER DATABASE [DBNoiThat] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [DBNoiThat] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [DBNoiThat] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [DBNoiThat] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [DBNoiThat] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [DBNoiThat] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [DBNoiThat] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [DBNoiThat] SET RECOVERY FULL 
GO
ALTER DATABASE [DBNoiThat] SET  MULTI_USER 
GO
ALTER DATABASE [DBNoiThat] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [DBNoiThat] SET DB_CHAINING OFF 
GO
ALTER DATABASE [DBNoiThat] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [DBNoiThat] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [DBNoiThat] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [DBNoiThat] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
EXEC sys.sp_db_vardecimal_storage_format N'DBNoiThat', N'ON'
GO
ALTER DATABASE [DBNoiThat] SET QUERY_STORE = ON
GO
ALTER DATABASE [DBNoiThat] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [DBNoiThat]
GO
/****** Object:  User [cdc]    Script Date: 02/01/2026 2:47:05 CH ******/
CREATE USER [cdc] WITHOUT LOGIN WITH DEFAULT_SCHEMA=[cdc]
GO
ALTER ROLE [db_owner] ADD MEMBER [cdc]
GO
/****** Object:  Schema [cdc]    Script Date: 02/01/2026 2:47:05 CH ******/
CREATE SCHEMA [cdc]
GO
/****** Object:  UserDefinedFunction [cdc].[fn_cdc_get_all_changes_dbo_Product]    Script Date: 02/01/2026 2:47:05 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

	create function [cdc].[fn_cdc_get_all_changes_dbo_Product]
	(	@from_lsn binary(10),
		@to_lsn binary(10),
		@row_filter_option nvarchar(30)
	)
	returns table
	return
	
	select NULL as __$start_lsn,
		NULL as __$seqval,
		NULL as __$operation,
		NULL as __$update_mask, NULL as [ProductId], NULL as [Name], NULL as [Description], NULL as [Price], NULL as [Quantity], NULL as [ProviderId], NULL as [CateId], NULL as [Photo], NULL as [StartDate], NULL as [EndDate], NULL as [Discount]
	where ( [sys].[fn_cdc_check_parameters]( N'dbo_Product', @from_lsn, @to_lsn, lower(rtrim(ltrim(@row_filter_option))), 0) = 0)

	union all
	
	select t.__$start_lsn as __$start_lsn,
		t.__$seqval as __$seqval,
		t.__$operation as __$operation,
		t.__$update_mask as __$update_mask, t.[ProductId], t.[Name], t.[Description], t.[Price], t.[Quantity], t.[ProviderId], t.[CateId], t.[Photo], t.[StartDate], t.[EndDate], t.[Discount]
	from [cdc].[dbo_Product_CT] t with (nolock)    
	where (lower(rtrim(ltrim(@row_filter_option))) = 'all')
	    and ( [sys].[fn_cdc_check_parameters]( N'dbo_Product', @from_lsn, @to_lsn, lower(rtrim(ltrim(@row_filter_option))), 0) = 1)
		and (t.__$operation = 1 or t.__$operation = 2 or t.__$operation = 4)
		and (t.__$start_lsn <= @to_lsn)
		and (t.__$start_lsn >= @from_lsn)
		
	union all	
		
	select t.__$start_lsn as __$start_lsn,
		t.__$seqval as __$seqval,
		t.__$operation as __$operation,
		t.__$update_mask as __$update_mask, t.[ProductId], t.[Name], t.[Description], t.[Price], t.[Quantity], t.[ProviderId], t.[CateId], t.[Photo], t.[StartDate], t.[EndDate], t.[Discount]
	from [cdc].[dbo_Product_CT] t with (nolock)     
	where (lower(rtrim(ltrim(@row_filter_option))) = 'all update old')
	    and ( [sys].[fn_cdc_check_parameters]( N'dbo_Product', @from_lsn, @to_lsn, lower(rtrim(ltrim(@row_filter_option))), 0) = 1)
		and (t.__$operation = 1 or t.__$operation = 2 or t.__$operation = 4 or
		     t.__$operation = 3 )
		and (t.__$start_lsn <= @to_lsn)
		and (t.__$start_lsn >= @from_lsn)
	
GO
/****** Object:  UserDefinedFunction [cdc].[fn_cdc_get_net_changes_dbo_Product]    Script Date: 02/01/2026 2:47:05 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

	create function [cdc].[fn_cdc_get_net_changes_dbo_Product]
	(	@from_lsn binary(10),
		@to_lsn binary(10),
		@row_filter_option nvarchar(30)
	)
	returns table
	return

	select NULL as __$start_lsn,
		NULL as __$operation,
		NULL as __$update_mask, NULL as [ProductId], NULL as [Name], NULL as [Description], NULL as [Price], NULL as [Quantity], NULL as [ProviderId], NULL as [CateId], NULL as [Photo], NULL as [StartDate], NULL as [EndDate], NULL as [Discount]
	where ( [sys].[fn_cdc_check_parameters]( N'dbo_Product', @from_lsn, @to_lsn, lower(rtrim(ltrim(@row_filter_option))), 1) = 0)

	union all
	
	select __$start_lsn,
	    case __$count_E146EA91
	    when 1 then __$operation
	    else
			case __$min_op_E146EA91 
				when 2 then 2
				when 4 then
				case __$operation
					when 1 then 1
					else 4
					end
				else
				case __$operation
					when 2 then 4
					when 4 then 4
					else 1
					end
			end
		end as __$operation,
		null as __$update_mask , [ProductId], [Name], [Description], [Price], [Quantity], [ProviderId], [CateId], [Photo], [StartDate], [EndDate], [Discount]
	from
	(
		select t.__$start_lsn as __$start_lsn, __$operation,
		case __$count_E146EA91 
		when 1 then __$operation 
		else
		(	select top 1 c.__$operation
			from [cdc].[dbo_Product_CT] c with (nolock)   
			where  ( (c.[ProductId] = t.[ProductId]) )  
			and ((c.__$operation = 2) or (c.__$operation = 4) or (c.__$operation = 1))
			and (c.__$start_lsn <= @to_lsn)
			and (c.__$start_lsn >= @from_lsn)
			order by c.__$start_lsn, c.__$command_id, c.__$seqval) end __$min_op_E146EA91, __$count_E146EA91, t.[ProductId], t.[Name], t.[Description], t.[Price], t.[Quantity], t.[ProviderId], t.[CateId], t.[Photo], t.[StartDate], t.[EndDate], t.[Discount] 
		from [cdc].[dbo_Product_CT] t with (nolock) inner join 
		(	select  r.[ProductId],
		    count(*) as __$count_E146EA91 
			from [cdc].[dbo_Product_CT] r with (nolock)
			where  (r.__$start_lsn <= @to_lsn)
			and (r.__$start_lsn >= @from_lsn)
			group by   r.[ProductId]) m
		on t.__$seqval = ( select top 1 c.__$seqval from [cdc].[dbo_Product_CT] c with (nolock) where  ( (c.[ProductId] = t.[ProductId]) )  and c.__$start_lsn <= @to_lsn and c.__$start_lsn >= @from_lsn order by c.__$start_lsn desc, c.__$command_id desc, c.__$seqval desc ) and
		    ( (t.[ProductId] = m.[ProductId]) ) 	
		where lower(rtrim(ltrim(@row_filter_option))) = N'all'
			and ( [sys].[fn_cdc_check_parameters]( N'dbo_Product', @from_lsn, @to_lsn, lower(rtrim(ltrim(@row_filter_option))), 1) = 1)
			and (t.__$start_lsn <= @to_lsn)
			and (t.__$start_lsn >= @from_lsn)
			and ((t.__$operation = 2) or (t.__$operation = 4) or 
				 ((t.__$operation = 1) and
				  (2 not in 
				 		(	select top 1 c.__$operation
							from [cdc].[dbo_Product_CT] c with (nolock) 
							where  ( (c.[ProductId] = t.[ProductId]) )  
							and ((c.__$operation = 2) or (c.__$operation = 4) or (c.__$operation = 1))
							and (c.__$start_lsn <= @to_lsn)
							and (c.__$start_lsn >= @from_lsn)
							order by c.__$start_lsn, c.__$command_id, c.__$seqval
						 ) 
	 			   )
	 			 )
	 			) 
			and t.__$operation = (
				select
					max(mo.__$operation)
				from
					[cdc].[dbo_Product_CT] as mo with (nolock)
				where
					mo.__$seqval = t.__$seqval
					and 
					 ( (t.[ProductId] = mo.[ProductId]) ) 
				group by
					mo.__$seqval
			)	
	) Q
	
	union all
	
	select __$start_lsn,
	    case __$count_E146EA91
	    when 1 then __$operation
	    else
			case __$min_op_E146EA91 
				when 2 then 2
				when 4 then
				case __$operation
					when 1 then 1
					else 4
					end
				else
				case __$operation
					when 2 then 4
					when 4 then 4
					else 1
					end
			end
		end as __$operation,
		case __$count_E146EA91
		when 1 then
			case __$operation
			when 4 then __$update_mask
			else null
			end
		else	
			case __$min_op_E146EA91 
			when 2 then null
			else
				case __$operation
				when 1 then null
				else __$update_mask 
				end
			end	
		end as __$update_mask , [ProductId], [Name], [Description], [Price], [Quantity], [ProviderId], [CateId], [Photo], [StartDate], [EndDate], [Discount]
	from
	(
		select t.__$start_lsn as __$start_lsn, __$operation,
		case __$count_E146EA91 
		when 1 then __$operation 
		else
		(	select top 1 c.__$operation
			from [cdc].[dbo_Product_CT] c with (nolock)
			where  ( (c.[ProductId] = t.[ProductId]) )  
			and ((c.__$operation = 2) or (c.__$operation = 4) or (c.__$operation = 1))
			and (c.__$start_lsn <= @to_lsn)
			and (c.__$start_lsn >= @from_lsn)
			order by c.__$start_lsn, c.__$command_id, c.__$seqval) end __$min_op_E146EA91, __$count_E146EA91, 
		m.__$update_mask , t.[ProductId], t.[Name], t.[Description], t.[Price], t.[Quantity], t.[ProviderId], t.[CateId], t.[Photo], t.[StartDate], t.[EndDate], t.[Discount]
		from [cdc].[dbo_Product_CT] t with (nolock) inner join 
		(	select  r.[ProductId],
		    count(*) as __$count_E146EA91, 
		    CAST(NULL AS varbinary(128)) as __$update_mask
			from [cdc].[dbo_Product_CT] r with (nolock)
			where  (r.__$start_lsn <= @to_lsn)
			and (r.__$start_lsn >= @from_lsn)
			group by   r.[ProductId]) m
		on t.__$seqval = ( select top 1 c.__$seqval from [cdc].[dbo_Product_CT] c with (nolock) where  ( (c.[ProductId] = t.[ProductId]) )  and c.__$start_lsn <= @to_lsn and c.__$start_lsn >= @from_lsn order by c.__$start_lsn desc, c.__$command_id desc, c.__$seqval desc ) and
		    ( (t.[ProductId] = m.[ProductId]) ) 	
		where lower(rtrim(ltrim(@row_filter_option))) = N'all with mask'
			and ( [sys].[fn_cdc_check_parameters]( N'dbo_Product', @from_lsn, @to_lsn, lower(rtrim(ltrim(@row_filter_option))), 1) = 1)
			and (t.__$start_lsn <= @to_lsn)
			and (t.__$start_lsn >= @from_lsn)
			and ((t.__$operation = 2) or (t.__$operation = 4) or 
				 ((t.__$operation = 1) and
				  (2 not in 
				 		(	select top 1 c.__$operation
							from [cdc].[dbo_Product_CT] c with (nolock)
							where  ( (c.[ProductId] = t.[ProductId]) )  
							and ((c.__$operation = 2) or (c.__$operation = 4) or (c.__$operation = 1))
							and (c.__$start_lsn <= @to_lsn)
							and (c.__$start_lsn >= @from_lsn)
							order by c.__$start_lsn, c.__$command_id, c.__$seqval
						 ) 
	 			   )
	 			 )
	 			) 
			and t.__$operation = (
				select
					max(mo.__$operation)
				from
					[cdc].[dbo_Product_CT] as mo with (nolock)
				where
					mo.__$seqval = t.__$seqval
					and 
					 ( (t.[ProductId] = mo.[ProductId]) ) 
				group by
					mo.__$seqval
			)	
	) Q
	
	union all
	
		select t.__$start_lsn as __$start_lsn,
		case t.__$operation
			when 1 then 1
			else 5
		end as __$operation,
		null as __$update_mask , t.[ProductId], t.[Name], t.[Description], t.[Price], t.[Quantity], t.[ProviderId], t.[CateId], t.[Photo], t.[StartDate], t.[EndDate], t.[Discount]
		from [cdc].[dbo_Product_CT] t  with (nolock)
		where lower(rtrim(ltrim(@row_filter_option))) = N'all with merge'
			and ( [sys].[fn_cdc_check_parameters]( N'dbo_Product', @from_lsn, @to_lsn, lower(rtrim(ltrim(@row_filter_option))), 1) = 1)
			and (t.__$start_lsn <= @to_lsn)
			and (t.__$start_lsn >= @from_lsn)
			and (t.__$seqval = ( select top 1 c.__$seqval from [cdc].[dbo_Product_CT] c with (nolock) where  ( (c.[ProductId] = t.[ProductId]) )  and c.__$start_lsn <= @to_lsn and c.__$start_lsn >= @from_lsn order by c.__$start_lsn desc, c.__$command_id desc, c.__$seqval desc ))
			and ((t.__$operation = 2) or (t.__$operation = 4) or 
				 ((t.__$operation = 1) and 
				   (2 not in 
				 		(	select top 1 c.__$operation
							from [cdc].[dbo_Product_CT] c with (nolock)
							where  ( (c.[ProductId] = t.[ProductId]) )  
							and ((c.__$operation = 2) or (c.__$operation = 4) or (c.__$operation = 1))
							and (c.__$start_lsn <= @to_lsn)
							and (c.__$start_lsn >= @from_lsn)
							order by c.__$start_lsn, c.__$command_id, c.__$seqval
						 ) 
	 				)
	 			 )
	 			)
			and t.__$operation = (
				select
					max(mo.__$operation)
				from
					[cdc].[dbo_Product_CT] as mo with (nolock)
				where
					mo.__$seqval = t.__$seqval
					and 
					 ( (t.[ProductId] = mo.[ProductId]) ) 
				group by
					mo.__$seqval
			)
	 
GO
/****** Object:  Table [dbo].[AnhChiTiet]    Script Date: 02/01/2026 2:47:05 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AnhChiTiet](
	[ProductId] [int] NULL,
	[Photo] [nvarchar](max) NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL,
 CONSTRAINT [PK_AnhChiTiet] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[AnhChiTiet] SET (LOCK_ESCALATION = AUTO)
GO
/****** Object:  Table [dbo].[Card]    Script Date: 02/01/2026 2:47:05 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Card](
	[CardId] [int] IDENTITY(1,1) NOT NULL,
	[NumberCard] [int] NULL,
	[UserNumber] [int] NULL,
	[UserId] [int] NULL,
	[Identification] [int] NULL,
 CONSTRAINT [PK_Card] PRIMARY KEY CLUSTERED 
(
	[CardId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Category]    Script Date: 02/01/2026 2:47:05 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Category](
	[CategoryId] [int] NOT NULL,
	[Name] [nvarchar](50) NULL,
	[MetaTitle] [nvarchar](50) NULL,
	[ParId] [int] NULL,
 CONSTRAINT [PK_Category] PRIMARY KEY CLUSTERED 
(
	[CategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Contact]    Script Date: 02/01/2026 2:47:05 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Contact](
	[ContactId] [int] IDENTITY(1,1) NOT NULL,
	[Content] [nvarchar](max) NULL,
	[Status] [bit] NULL,
	[EmailCC] [nvarchar](50) NULL,
 CONSTRAINT [PK_Contact] PRIMARY KEY CLUSTERED 
(
	[ContactId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Credentials]    Script Date: 02/01/2026 2:47:05 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Credentials](
	[CredenId] [int] IDENTITY(1,1) NOT NULL,
	[UserGroupId] [nvarchar](50) NULL,
	[RoleId] [nvarchar](50) NULL,
 CONSTRAINT [PK_Credentials] PRIMARY KEY CLUSTERED 
(
	[CredenId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[News]    Script Date: 02/01/2026 2:47:05 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[News](
	[NewsId] [int] NOT NULL,
	[Title] [nvarchar](max) NULL,
	[Detail] [nvarchar](max) NULL,
	[Photo] [nvarchar](max) NULL,
	[DateUpdate] [date] NULL,
 CONSTRAINT [PK_News] PRIMARY KEY CLUSTERED 
(
	[NewsId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Order]    Script Date: 02/01/2026 2:47:05 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Order](
	[OrderId] [int] IDENTITY(1,1) NOT NULL,
	[ShipName] [nvarchar](50) NULL,
	[UserId] [int] NULL,
	[ShipPhone] [int] NULL,
	[ShipEmail] [nvarchar](max) NULL,
	[UpdateDate] [datetime] NULL,
	[ShipAddress] [nvarchar](max) NULL,
	[StatusId] [int] NULL,
	[PaymentMethod] [nvarchar](50) NULL,
	[OrderTime] [datetime] NULL,
 CONSTRAINT [PK_Order] PRIMARY KEY CLUSTERED 
(
	[OrderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderDetail]    Script Date: 02/01/2026 2:47:05 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderDetail](
	[OrderDetailId] [int] IDENTITY(1,1) NOT NULL,
	[OrderId] [int] NULL,
	[ProductId] [int] NULL,
	[Price] [int] NULL,
	[Quantity] [int] NULL,
 CONSTRAINT [PK_OrderDetailId] PRIMARY KEY CLUSTERED 
(
	[OrderDetailId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Product]    Script Date: 02/01/2026 2:47:05 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Product](
	[ProductId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Description] [nvarchar](max) NULL,
	[Price] [int] NULL,
	[Quantity] [int] NULL,
	[ProviderId] [int] NULL,
	[CateId] [int] NULL,
	[Photo] [nvarchar](max) NULL,
	[StartDate] [date] NULL,
	[EndDate] [date] NULL,
	[Discount] [int] NULL,
 CONSTRAINT [PK_Product] PRIMARY KEY CLUSTERED 
(
	[ProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductOrder]    Script Date: 02/01/2026 2:47:05 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductOrder](
	[ProductOrderId] [int] IDENTITY(1,1) NOT NULL,
	[ProductId] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Price] [int] NOT NULL,
	[Discount] [int] NULL,
	[OrderDetailId] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ProductOrderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Provider]    Script Date: 02/01/2026 2:47:05 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Provider](
	[ProviderId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Address] [nvarchar](max) NULL,
	[Phone] [int] NULL,
 CONSTRAINT [PK_Provider] PRIMARY KEY CLUSTERED 
(
	[ProviderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Role]    Script Date: 02/01/2026 2:47:05 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Role](
	[RoleId] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](50) NULL,
 CONSTRAINT [PK_Role] PRIMARY KEY CLUSTERED 
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Status]    Script Date: 02/01/2026 2:47:05 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Status](
	[StatusId] [int] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[StatusId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[User]    Script Date: 02/01/2026 2:47:05 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[User](
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Address] [nvarchar](50) NULL,
	[Phone] [int] NULL,
	[Username] [nvarchar](50) NULL,
	[Password] [nvarchar](50) NULL,
	[Email] [nvarchar](max) NULL,
	[GroupId] [nvarchar](50) NULL,
	[Status] [bit] NOT NULL,
 CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserGroups]    Script Date: 02/01/2026 2:47:05 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserGroups](
	[GroupId] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](50) NULL,
 CONSTRAINT [PK_UserGroups] PRIMARY KEY CLUSTERED 
(
	[GroupId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AnhChiTiet]  WITH CHECK ADD  CONSTRAINT [FK_AnhChiTiet_Product] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Product] ([ProductId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AnhChiTiet] CHECK CONSTRAINT [FK_AnhChiTiet_Product]
GO
USE [master]
GO
ALTER DATABASE [DBNoiThat] SET  READ_WRITE 
GO
