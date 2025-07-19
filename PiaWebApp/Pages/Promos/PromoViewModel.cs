using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using PiaWebApp.Data;
using System.Globalization;
using System.Threading;

namespace PiaWebApp.Pages.Promos
{
    public class PromoViewModel : IDisposable
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly NavigationManager _navigation;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ApplicationDbContext _dbContext;
        private CancellationTokenSource _debounceTokenSource = new();

        public PromoViewModel(
            IJSRuntime jsRuntime,
            NavigationManager navigation,
            AuthenticationStateProvider authStateProvider,
            ApplicationDbContext dbContext)
        {
            _jsRuntime = jsRuntime;
            _navigation = navigation;
            _authStateProvider = authStateProvider;
            _dbContext = dbContext;
        }

        // Store Information
        public string StoreCode { get; private set; } = string.Empty;
        public string StoreDesc { get; private set; } = string.Empty;

        // Product Information
        public string Sku { get; set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public decimal SrpValue { get; private set; }
        public string SrpFormatted { get; private set; } = string.Empty;

        // Promo Information
        public string PromoType { get; set; } = string.Empty;
        public List<string> AvailablePromoTitles { get; private set; } = new();
        public long CurrentPromoNo { get; private set; }

        // Payment Information
        public string PaymentMode { get; set; } = string.Empty;
        public List<string> AvailableMops { get; private set; } = new();
        public int Monthx { get; private set; }
        public decimal ItemDiscountValue { get; private set; }
        public decimal ExtendedDiscountValue { get; private set; }
        public string ItemDiscountFormatted { get; private set; } = string.Empty;
        public string ExtendedDiscountFormatted { get; private set; } = string.Empty;

        // Payment Display
        public string PayOnly2 { get; private set; } = string.Empty;
        public string PayOnly3 { get; private set; } = string.Empty;
        public string Save2 { get; private set; } = string.Empty;
        public bool ShowPayOnly3 { get; private set; }

        // Free Items Sections
        public string FreeItemsUpperSection { get; private set; } = string.Empty;
        public string FreeItemsLowerSection { get; private set; } = string.Empty;
        public string SelectedFreeItem { get; set; } = string.Empty;
        public List<string> AvailableFreeItems { get; private set; } = new();

        // Screen Information
        public string ScreenWidth { get; private set; } = "0px";
        public string ScreenHeight { get; private set; } = "0px";

        public string CashPaymentHeader =>
            PaymentMode.Equals("Cash/Card Straight", StringComparison.OrdinalIgnoreCase)
                ? "CASH/CARD STRAIGHT PAYMENT"
                : PaymentMode.ToUpper();

        public async Task InitializeAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            if (!authState.User.Identity.IsAuthenticated)
            {
                _navigation.NavigateTo("/Login");
                return;
            }

            StoreCode = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "storeCode");
            StoreDesc = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "storeDesc");

            if (string.IsNullOrEmpty(StoreCode) || string.IsNullOrEmpty(StoreDesc))
            {
                _navigation.NavigateTo("/Login");
            }
        }

        public async Task InitializeScreenSize()
        {
            try
            {
                var dimensions = await _jsRuntime.InvokeAsync<int[]>("getScreenDimensions");
                ScreenWidth = $"{dimensions[0]}px";
                ScreenHeight = $"{dimensions[1]}px";
            }
            catch
            {
                ScreenWidth = ScreenHeight = "N/A";
            }
        }

        public async Task HandleSkuChanged(string newSku)
        {
            Sku = newSku.Trim();
            ClearPromoData();

            _debounceTokenSource.Cancel();
            _debounceTokenSource = new CancellationTokenSource();

            try
            {
                await Task.Delay(500, _debounceTokenSource.Token);

                if (string.IsNullOrEmpty(Sku))
                {
                    return;
                }

                if (!int.TryParse(Sku, out int parsedSku))
                {
                    Description = "Invalid SKU format";
                    return;
                }

                await LoadPromoData(parsedSku);
            }
            catch (TaskCanceledException)
            {
                // Ignore canceled tasks
            }
            catch (Exception ex)
            {
                Description = "Error loading data: " + ex.Message;
            }
        }

        private void ClearPromoData()
        {
            AvailablePromoTitles.Clear();
            AvailableMops.Clear();
            AvailableFreeItems.Clear();
            PromoType = string.Empty;
            PaymentMode = string.Empty;
            SelectedFreeItem = string.Empty;
            Description = string.Empty;
            SrpFormatted = string.Empty;
            ItemDiscountFormatted = string.Empty;
            ExtendedDiscountFormatted = string.Empty;
            PayOnly2 = string.Empty;
            PayOnly3 = string.Empty;
            Save2 = string.Empty;
            FreeItemsUpperSection = string.Empty;
            FreeItemsLowerSection = string.Empty;
            CurrentPromoNo = 0;
            Monthx = 0;
            SrpValue = 0;
            ItemDiscountValue = 0;
            ExtendedDiscountValue = 0;
            ShowPayOnly3 = false;
        }

        private async Task LoadPromoData(int parsedSku)
        {
            try
            {
                DateTime today = DateTime.Now.Date;

                var allPromos = await _dbContext.Promos
                    .Where(p => p.Sku == parsedSku)
                    .Where(p => today >= p.StartDate.Date && today <= p.EndDate.Date)
                    .Where(p => p.storecodex == StoreCode || p.storecodex == "All")
                    .ToListAsync();

                var activePromos = allPromos
                    .GroupBy(p => p.Title)
                    .Select(g => g.OrderByDescending(p => p.PromoNo).First())
                    .ToList();

                if (!activePromos.Any())
                {
                    Description = "No active promos found for this SKU";
                    return;
                }

                var currentPromo = activePromos.OrderByDescending(p => p.PromoNo).First();
                Description = $"{currentPromo.subcategory} {currentPromo.Description}\n{currentPromo.capacity}";
                CurrentPromoNo = currentPromo.PromoNo;

                if (decimal.TryParse(currentPromo.Srp, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsedSrp))
                {
                    SrpValue = parsedSrp;
                    var nfi = new CultureInfo("en-PH").NumberFormat;
                    nfi.CurrencySymbol = "₱";
                    nfi.CurrencyDecimalDigits = 2;
                    SrpFormatted = SrpValue.ToString("C", nfi);
                }
                else
                {
                    SrpFormatted = "Invalid SRP format";
                }

                AvailablePromoTitles = activePromos.Select(p => p.Title).Distinct().ToList();
                PromoType = currentPromo.Title;
                await LoadPromoDetails(parsedSku, PromoType);
            }
            catch (Exception ex)
            {
                Description = "Error loading promo data: " + ex.Message;
            }
        }

        public async Task HandlePromoTypeChanged(string newPromoType)
        {
            PromoType = newPromoType.Trim();
            AvailableMops.Clear();
            AvailableFreeItems.Clear();
            PaymentMode = string.Empty;
            SelectedFreeItem = string.Empty;
            ItemDiscountFormatted = string.Empty;
            ExtendedDiscountFormatted = string.Empty;
            PayOnly2 = string.Empty;
            PayOnly3 = string.Empty;
            Save2 = string.Empty;
            FreeItemsUpperSection = string.Empty;
            FreeItemsLowerSection = string.Empty;
            ItemDiscountValue = 0;
            ExtendedDiscountValue = 0;
            ShowPayOnly3 = false;

            if (string.IsNullOrEmpty(PromoType) || !int.TryParse(Sku, out int parsedSku))
            {
                return;
            }

            await LoadPromoDetails(parsedSku, PromoType);
        }

        public async Task HandleMopChanged(string newPaymentMode)
        {
            PaymentMode = newPaymentMode.Trim();
            SelectedFreeItem = string.Empty;
            PayOnly2 = string.Empty;
            PayOnly3 = string.Empty;
            Save2 = string.Empty;
            FreeItemsUpperSection = string.Empty;
            FreeItemsLowerSection = string.Empty;
            ShowPayOnly3 = false;

            if (!string.IsNullOrEmpty(PaymentMode) && !string.IsNullOrEmpty(PromoType) && int.TryParse(Sku, out int parsedSku))
            {
                await LoadDiscountValues(parsedSku, PromoType, PaymentMode);
                await LoadFreeItems(parsedSku, PromoType, PaymentMode);
                CalculatePaymentFields();
            }
        }

        private async Task LoadPromoDetails(int sku, string title)
        {
            try
            {
                DateTime today = DateTime.Now.Date;
                var promo = await _dbContext.Promos
                    .FirstOrDefaultAsync(p => p.Sku == sku
                        && p.Title == title
                        && today >= p.StartDate.Date
                        && today <= p.EndDate.Date
                        && (p.storecodex == StoreCode || p.storecodex == "All"));

                if (promo != null)
                {
                    CurrentPromoNo = promo.PromoNo;
                }

                var mops = await _dbContext.PromoDetails
                    .Where(p => p.PromoNo == CurrentPromoNo)
                    .Where(p => p.Sku == sku)
                    .Where(p => p.Title == title)
                    .Select(p => p.Mop)
                    .Distinct()
                    .ToListAsync();

                AvailableMops = mops;

                if (AvailableMops.Count == 1)
                {
                    PaymentMode = AvailableMops.First();
                    await LoadDiscountValues(sku, title, PaymentMode);
                    await LoadFreeItems(sku, title, PaymentMode);
                }
            }
            catch (Exception ex)
            {
                Description = "Error loading promo details: " + ex.Message;
            }
        }

        private async Task LoadDiscountValues(int sku, string title, string mop)
        {
            try
            {
                var promoDetail = await _dbContext.PromoDetails
                    .FirstOrDefaultAsync(pd => pd.PromoNo == CurrentPromoNo
                        && pd.Sku == sku
                        && pd.Title == title
                        && pd.Mop == mop);

                if (promoDetail != null)
                {
                    var nfi = new CultureInfo("en-PH").NumberFormat;
                    nfi.CurrencySymbol = "₱";
                    nfi.CurrencyDecimalDigits = 2;

                    ItemDiscountValue = Convert.ToDecimal(promoDetail.Discount);
                    ExtendedDiscountValue = Convert.ToDecimal(promoDetail.Extended);
                    Monthx = promoDetail.monthx;

                    ItemDiscountFormatted = ItemDiscountValue.ToString("C", nfi);
                    ExtendedDiscountFormatted = ExtendedDiscountValue.ToString("C", nfi);

                    CalculatePaymentFields();
                }
                else
                {
                    ItemDiscountFormatted = string.Empty;
                    ExtendedDiscountFormatted = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Description = "Error loading discount values: " + ex.Message;
            }
        }

        private void CalculatePaymentFields()
        {
            try
            {
                var nfi = new CultureInfo("en-PH").NumberFormat;
                nfi.CurrencySymbol = "₱";
                nfi.CurrencyDecimalDigits = 2;

                decimal totalDiscount = ItemDiscountValue + ExtendedDiscountValue;
                decimal payOnlyAmount = SrpValue - totalDiscount;

                ShowPayOnly3 = !PaymentMode.StartsWith("Cash", StringComparison.OrdinalIgnoreCase) && Monthx > 0;

                Save2 = totalDiscount.ToString("C", nfi);

                if (Monthx > 0)
                {
                    decimal monthlyPayment = payOnlyAmount / Monthx;
                    PayOnly2 = $"PAY ONLY: {payOnlyAmount.ToString("C", nfi)}";

                    if (ShowPayOnly3)
                    {
                        PayOnly3 = $"{monthlyPayment.ToString("C", nfi)} x {Monthx} month(s)";
                    }
                }
                else
                {
                    PayOnly2 = $"PAY ONLY: {payOnlyAmount.ToString("C", nfi)}";
                    PayOnly3 = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating payment fields: {ex.Message}");
            }
        }

        private async Task LoadFreeItems(int sku, string title, string mop)
        {
            try
            {
                var promo = await _dbContext.Promos
                    .FirstOrDefaultAsync(p => p.PromoNo == CurrentPromoNo
                        && p.Sku == sku
                        && p.Title == title);

                if (promo != null)
                {
                    // Process upper section (Data18, Data19, Data21, Data22, Data23)
                    var upperValues = new List<string>();
                    if (!string.IsNullOrEmpty(promo.Data18))
                        upperValues.Add(promo.Data18 == "None" ? "None" : promo.Data18);
                    if (!string.IsNullOrEmpty(promo.Data19))
                        upperValues.Add(promo.Data19 == "None" ? "None" : promo.Data19);
                    if (!string.IsNullOrEmpty(promo.Data21))
                        upperValues.Add(promo.Data21 == "None" ? "None" : promo.Data21);
                    if (!string.IsNullOrEmpty(promo.Data22))
                        upperValues.Add(promo.Data22 == "None" ? "None" : promo.Data22);
                    if (!string.IsNullOrEmpty(promo.Data23))
                        upperValues.Add(promo.Data23 == "None" ? "None" : promo.Data23);

                    // Filter out "-" values but keep "None" as literal text
                    FreeItemsUpperSection = string.Join("\n", upperValues
                        .Where(v => !string.IsNullOrWhiteSpace(v) &&
                                   !v.Equals("-", StringComparison.OrdinalIgnoreCase)));

                    // Process lower section (Data31, Data20)
                    var lowerValues = new List<string>();
                    if (!string.IsNullOrEmpty(promo.Data31))
                        lowerValues.Add(promo.Data31 == "None" ? "None" : promo.Data31);
                    if (!string.IsNullOrEmpty(promo.Data20))
                        lowerValues.Add(promo.Data20 == "None" ? "None" : promo.Data20);

                    // Filter out "-" values but keep "None" as literal text
                    FreeItemsLowerSection = string.Join("\n", lowerValues
                        .Where(v => !string.IsNullOrWhiteSpace(v) &&
                                   !v.Equals("-", StringComparison.OrdinalIgnoreCase)));

                    // Load the regular free items for the combobox
                    if (!string.IsNullOrEmpty(promo.PromoFreebies))
                    {
                        AvailableFreeItems = promo.PromoFreebies
                            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(f => f.Trim())
                            .ToList();
                    }
                }
                else
                {
                    FreeItemsUpperSection = string.Empty;
                    FreeItemsLowerSection = string.Empty;
                    AvailableFreeItems = new List<string>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading free items: {ex.Message}");
                FreeItemsUpperSection = string.Empty;
                FreeItemsLowerSection = string.Empty;
                AvailableFreeItems = new List<string>();
            }
        }

        public void Dispose()
        {
            _debounceTokenSource?.Cancel();
            _debounceTokenSource?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}