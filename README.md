# 房源管理與買賣配對系統

C# / WPF / .NET 8 / Entity Framework Core / SQLite 單機房源管理程式。

## 已包含功能

- 買方新增、修改、刪除、查詢清單
- 賣方房源新增、修改、刪除、查詢清單
- 買方與賣方欄位：姓名、手機號碼、LINE ID、價格、地址、房型、格局、其他備註
- 房型、格局為多對多關聯，可輸入多筆，也可保持空集合
- 價格、地址、房型、格局空白時代表不限
- 依買方尋找賣方房源
- 依賣方房源尋找買方
- 地址正規化與 Dice 二字組相似度
- 地址相似度門檻可調整
- SQLite 自動建庫，無須安裝資料庫伺服器

## 配對規則

符合結果必須同時通過：

1. 價格：買方最高預算空白、賣方價格空白，或賣方價格小於等於買方最高預算。
2. 房型：任一方為空集合，或雙方至少一個房型相同。
3. 格局：任一方為空集合，或雙方至少一個格局相同。
4. 地址：任一方地址空白，或地址相似度高於畫面設定門檻。

總分權重：地址 45%、房型 20%、格局 20%、價格 15%。

## 地址相似度

`AddressSimilarityService` 會：

- 將全形字元轉為相容字元
- 將「臺」統一為「台」
- 移除空白、標點符號
- 移除「台灣省」
- 使用字元二字組 Dice coefficient
- 短地址完整包含於長地址時提高分數，例如「板橋區」對「新北市板橋區文化路一段100號」
- 縣市或行政區明確不一致時降低分數

預設門檻為 45%。正式使用可依實際資料調整到 40%～65%。

## 開發環境

1. Windows 10 或 Windows 11
2. Visual Studio 2022
3. 安裝「.NET 桌面開發」工作負載
4. .NET 8 SDK

## 開啟與執行

1. 解壓縮專案。
2. 開啟 `RealEstateMatcher.sln`。
3. Visual Studio 還原 NuGet 套件。
4. 按 `F5` 執行。

命令列：

```powershell
cd RealEstateMatcher
dotnet restore
dotnet run --project .\RealEstateMatcher\RealEstateMatcher.csproj
```

也可以直接執行根目錄的 `run.bat`。

## 資料庫位置

程式第一次啟動會自動建立：

```text
%LocalAppData%\RealEstateMatcher\realestate.db
```

備份時關閉程式，複製 `realestate.db` 即可。

## 發布 EXE

執行：

```text
publish-win-x64.bat
```

輸出位置：

```text
publish\win-x64
```

目前發布設定為 framework-dependent，需要目標電腦安裝 .NET 8 Desktop Runtime。若要完全免安裝 Runtime，可將批次檔中的：

```text
--self-contained false
```

改成：

```text
--self-contained true
```

## 資料設計說明

實際資料表包含：

- Buyers
- SellerListings
- HouseTypes
- Layouts
- BuyerHouseTypes
- BuyerLayouts
- SellerHouseTypes
- SellerLayouts

房型與格局不是一般一對多，而是買方/賣方各自與對照資料形成多對多。

## 注意事項

目前使用 `Database.EnsureCreated()`，適合第一版與單機程式。正式上線後若資料結構需要持續升級，建議改用 EF Core Migrations。
