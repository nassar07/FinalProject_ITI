using Mscc.GenerativeAI;
using NetTopologySuite.Geometries;
using Microsoft.EntityFrameworkCore;
using GeoPoint = NetTopologySuite.Geometries.Point;
using FinalProject_ITI.Models;
namespace FinalProject_ITI.Services
{
    public class EmbeddingService
    {
        private readonly GoogleAI _googleAI;
        private readonly ITIContext _dbContext;
        private readonly GeometryFactory _geometryFactory;
        private readonly ILogger<EmbeddingService> _logger;

        public EmbeddingService(IConfiguration configuration, ITIContext dbContext, ILogger<EmbeddingService> logger)
        {
            var apiKey = configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey)) throw new InvalidOperationException("Gemini API Key is not configured.");

            _googleAI = new GoogleAI(apiKey);
            _dbContext = dbContext;
            _geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
            _logger = logger;
        }

        public async Task<string> GetChatbotResponse(string userQuery)
        {
            try
            {
                var context = await FindMostRelevantContext(userQuery);

                var prompt = $@"You are an AI assistant. Answer the user's question based ONLY on the provided context.
    
    CONTEXT:
    ---
    {context}
    ---

    QUESTION: {userQuery}

    ANSWER:";

                _logger.LogInformation("Using model: gemini-1.5-flash");
                var chatModel = _googleAI.GenerativeModel(model: "gemini-1.5-flash");
                var response = await chatModel.GenerateContent(prompt);

                return response.Text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetChatbotResponse");
                return "عذراً، حدث خطأ في معالجة طلبك. يرجى المحاولة مرة أخرى.";
            }
        }

        // وظيفة جديدة لإنشاء embeddings لجميع البيانات
        public async Task IndexAllData()
        {
            try
            {
                _logger.LogInformation("Starting comprehensive data indexing");
                var embeddingModel = _googleAI.GenerativeModel(model: "embedding-001");

                // إنشاء embeddings للمنتجات
                await IndexProducts(embeddingModel);
                
                // إنشاء embeddings للعلامات التجارية
                await IndexBrands(embeddingModel);
                
                // إنشاء embeddings للفئات
                await IndexCategories(embeddingModel);
                
                // إنشاء embeddings للطلبات
                await IndexOrders(embeddingModel);
                
                // إنشاء embeddings للمراجعات
                await IndexReviews(embeddingModel);

                _logger.LogInformation("Comprehensive data indexing completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IndexAllData");
                throw;
            }
        }

        private async Task IndexProducts(GenerativeModel embeddingModel)
        {
            try
            {
                _logger.LogInformation("Indexing products");
                var productsToIndex = await _dbContext.Products.Where(p => p.Embedding == null).ToListAsync();
                _logger.LogInformation("Found {Count} products to index", productsToIndex.Count);

                foreach (var product in productsToIndex)
                {
                    try
                    {
                        var textToEmbed = $"منتج: {product.Name} - وصف: {product.Description} - سعر: {product.Price}";
                        var response = await embeddingModel.EmbedContent(textToEmbed);
                        var vector = response.Embedding.Values.ToArray();
                        product.Embedding = _geometryFactory.CreatePoint(new Coordinate(vector[0], vector[1]));
                        _logger.LogInformation("Indexed product: {ProductName}", product.Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error indexing product: {ProductName}", product.Name);
                    }
                }
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IndexProducts");
            }
        }

        private async Task IndexBrands(GenerativeModel embeddingModel)
        {
            try
            {
                _logger.LogInformation("Indexing brands");
                var brandsToIndex = await _dbContext.Brands.Where(b => b.Embedding == null).ToListAsync();
                _logger.LogInformation("Found {Count} brands to index", brandsToIndex.Count);

                foreach (var brand in brandsToIndex)
                {
                    try
                    {
                        var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == brand.CategoryID);
                        var textToEmbed = $"علامة تجارية: {brand.Name} - وصف: {brand.Description} - عنوان: {brand.Address} - فئة: {category?.Name}";
                        var response = await embeddingModel.EmbedContent(textToEmbed);
                        var vector = response.Embedding.Values.ToArray();
                        brand.Embedding = _geometryFactory.CreatePoint(new Coordinate(vector[0], vector[1]));
                        _logger.LogInformation("Indexed brand: {BrandName}", brand.Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error indexing brand: {BrandName}", brand.Name);
                    }
                }
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IndexBrands");
            }
        }

        private async Task IndexCategories(GenerativeModel embeddingModel)
        {
            try
            {
                _logger.LogInformation("Indexing categories");
                var categoriesToIndex = await _dbContext.Categories.Where(c => c.Embedding == null).ToListAsync();
                _logger.LogInformation("Found {Count} categories to index", categoriesToIndex.Count);

                foreach (var category in categoriesToIndex)
                {
                    try
                    {
                        var textToEmbed = $"فئة: {category.Name}";
                        var response = await embeddingModel.EmbedContent(textToEmbed);
                        var vector = response.Embedding.Values.ToArray();
                        category.Embedding = _geometryFactory.CreatePoint(new Coordinate(vector[0], vector[1]));
                        _logger.LogInformation("Indexed category: {CategoryName}", category.Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error indexing category: {CategoryName}", category.Name);
                    }
                }
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IndexCategories");
            }
        }

        private async Task IndexOrders(GenerativeModel embeddingModel)
        {
            try
            {
                _logger.LogInformation("Indexing orders");
                var ordersToIndex = await _dbContext.Orders.Where(o => o.Embedding == null).ToListAsync();
                _logger.LogInformation("Found {Count} orders to index", ordersToIndex.Count);

                foreach (var order in ordersToIndex)
                {
                    try
                    {
                        var textToEmbed = $"طلب رقم: {order.Id} - تاريخ: {order.OrderDate} - الحالة: {order.Status} - المبلغ الإجمالي: {order.TotalAmount} - طريقة الدفع: {order.PaymentMethod}";
                        var response = await embeddingModel.EmbedContent(textToEmbed);
                        var vector = response.Embedding.Values.ToArray();
                        order.Embedding = _geometryFactory.CreatePoint(new Coordinate(vector[0], vector[1]));
                        _logger.LogInformation("Indexed order: {OrderId}", order.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error indexing order: {OrderId}", order.Id);
                    }
                }
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IndexOrders");
            }
        }

        private async Task IndexReviews(GenerativeModel embeddingModel)
        {
            try
            {
                _logger.LogInformation("Indexing reviews");
                var reviewsToIndex = await _dbContext.Reviews.Where(r => r.Embedding == null).ToListAsync();
                _logger.LogInformation("Found {Count} reviews to index", reviewsToIndex.Count);

                foreach (var review in reviewsToIndex)
                {
                    try
                    {
                        var textToEmbed = $"مراجعة: {review.Comment} - التقييم: {review.Rating}";
                        var response = await embeddingModel.EmbedContent(textToEmbed);
                        var vector = response.Embedding.Values.ToArray();
                        review.Embedding = _geometryFactory.CreatePoint(new Coordinate(vector[0], vector[1]));
                        _logger.LogInformation("Indexed review: {ReviewId}", review.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error indexing review: {ReviewId}", review.Id);
                    }
                }
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IndexReviews");
            }
        }

        // وظيفة قديمة للتوافق مع الكود الحالي
        public async Task IndexAllProducts()
        {
            await IndexAllData();
        }

        public async Task<string> FindMostRelevantContext(string userQuery)
        {
            try
            {
                _logger.LogInformation("Finding relevant context for query: {Query}", userQuery);
                var embeddingModel = _googleAI.GenerativeModel(model: "embedding-001");
                var queryResponse = await embeddingModel.EmbedContent(userQuery);
                var queryVector = queryResponse.Embedding.Values.ToArray();
                var queryPoint = _geometryFactory.CreatePoint(new Coordinate(queryVector[0], queryVector[1]));

                var contextParts = new List<string>();

                // البحث في المنتجات
                var topProducts = await _dbContext.Products
                    .Where(p => p.Embedding != null)
                    .OrderBy(p => p.Embedding.Distance(queryPoint))
                    .Take(2)
                    .ToListAsync();

                if (topProducts.Any())
                {
                    var productsContext = string.Join("\n---\n", topProducts.Select(p => $"منتج: {p.Name}\nالوصف: {p.Description}\nالسعر: {p.Price}"));
                    contextParts.Add($"المنتجات ذات الصلة:\n{productsContext}");
                }

                // البحث في العلامات التجارية
                var topBrands = await _dbContext.Brands
                    .Where(b => b.Embedding != null)
                    .OrderBy(b => b.Embedding.Distance(queryPoint))
                    .Take(2)
                    .ToListAsync();

                if (topBrands.Any())
                {
                    var brandsContext = string.Join("\n---\n", topBrands.Select(b => $"علامة تجارية: {b.Name}\nالوصف: {b.Description}\nالعنوان: {b.Address}"));
                    contextParts.Add($"العلامات التجارية ذات الصلة:\n{brandsContext}");
                }

                // البحث في الفئات
                var topCategories = await _dbContext.Categories
                    .Where(c => c.Embedding != null)
                    .OrderBy(c => c.Embedding.Distance(queryPoint))
                    .Take(2)
                    .ToListAsync();

                if (topCategories.Any())
                {
                    var categoriesContext = string.Join("\n---\n", topCategories.Select(c => $"فئة: {c.Name}"));
                    contextParts.Add($"الفئات ذات الصلة:\n{categoriesContext}");
                }

                // البحث في الطلبات
                var topOrders = await _dbContext.Orders
                    .Where(o => o.Embedding != null)
                    .OrderBy(o => o.Embedding.Distance(queryPoint))
                    .Take(2)
                    .ToListAsync();

                if (topOrders.Any())
                {
                    var ordersContext = string.Join("\n---\n", topOrders.Select(o => $"طلب رقم: {o.Id}\nالتاريخ: {o.OrderDate}\nالحالة: {o.Status}\nالمبلغ: {o.TotalAmount}"));
                    contextParts.Add($"الطلبات ذات الصلة:\n{ordersContext}");
                }

                // البحث في المراجعات
                var topReviews = await _dbContext.Reviews
                    .Where(r => r.Embedding != null)
                    .OrderBy(r => r.Embedding.Distance(queryPoint))
                    .Take(2)
                    .ToListAsync();

                if (topReviews.Any())
                {
                    var reviewsContext = string.Join("\n---\n", topReviews.Select(r => $"مراجعة: {r.Comment}\nالتقييم: {r.Rating}"));
                    contextParts.Add($"المراجعات ذات الصلة:\n{reviewsContext}");
                }

                if (!contextParts.Any())
                {
                    _logger.LogWarning("No relevant data found for query: {Query}", userQuery);
                    return "لم يتم العثور على معلومات ذات صلة في قاعدة البيانات.";
                }

                var context = string.Join("\n\n", contextParts);
                _logger.LogInformation("Found relevant data from multiple sources");
                return context;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FindMostRelevantContext");
                return "عذراً، حدث خطأ في البحث عن المعلومات ذات الصلة.";
            }
        }
    }
}

