using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinalProject_ITI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mscc.GenerativeAI;

namespace FinalProject_ITI.Services
{
    public class EmbeddingService
    {
        private readonly GoogleAI _googleAI;
        private readonly ITIContext _dbContext;
        private readonly ILogger<EmbeddingService> _logger;
        private readonly GenerativeModel _embeddingModel;

        public EmbeddingService(GoogleAI googleAI, ITIContext dbContext, ILogger<EmbeddingService> logger)
        {
            _googleAI = googleAI ?? throw new ArgumentNullException(nameof(googleAI));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger;
            _embeddingModel = _googleAI.GenerativeModel(model: "embedding-001"); // عدّل لو موديل مختلف
        }

        // Public method to index everything
        public async Task IndexAllDataAsync()
        {
            _logger.LogInformation("IndexAllDataAsync started");
            await IndexProductsAsync();
            await IndexBrandsAsync();
            await IndexCategoriesAsync();
            await IndexOrdersAsync();
            await IndexOrderDetailsAsync();
            await IndexReviewsAsync();
            await IndexSubscribesAsync();
            await IndexBazarsAsync();
            await IndexBazarBrandsAsync();
            await IndexPaymentsAsync();
            _logger.LogInformation("IndexAllDataAsync finished");
            await _dbContext.SaveChangesAsync();
        }

        // --------- Index helpers for each entity (build content and call Upsert) ----------
        private async Task IndexProductsAsync()
        {
            var items = await _dbContext.Products.AsNoTracking().ToListAsync();
            foreach (var p in items)
            {
                var content = $"منتج: {p.Name}\nالوصف: {p.Description}\nالسعر: {p.Price}\nالكمية: {p.Quantity}";
                await UpsertDocumentEmbeddingAsync("Product", p.Id.ToString(), $"Product:{p.Id} - {p.Name}", content);
            }
        }

        private async Task IndexBrandsAsync()
        {
            var items = await _dbContext.Brands.AsNoTracking().ToListAsync();
            foreach (var b in items)
            {
                var content = $"علامة تجارية: {b.Name}\nالوصف: {b.Description}\nالعنوان: {b.Address}";
                await UpsertDocumentEmbeddingAsync("Brand", b.Id.ToString(), $"Brand:{b.Id} - {b.Name}", content);
            }
        }

        private async Task IndexCategoriesAsync()
        {
            var items = await _dbContext.Categories.AsNoTracking().ToListAsync();
            foreach (var c in items)
            {
                var content = $"فئة: {c.Name}\nالوصف: {c.Id}";
                await UpsertDocumentEmbeddingAsync("Category", c.Id.ToString(), $"Category:{c.Id} - {c.Name}", content);
            }
        }

        private async Task IndexOrdersAsync()
        {
            var items = await _dbContext.Orders
                .Include(o => o.OrderDetails) // إذا عندك relation
                .AsNoTracking()
                .ToListAsync();

            foreach (var o in items)
            {
                var details = new StringBuilder();
                if (o.OrderDetails != null && o.OrderDetails.Any())
                {
                    foreach (var d in o.OrderDetails)
                    {
                        details.AppendLine($"منتجId:{d.ProductID} - كمية:{d.Quantity} - سعر:{d.Price}");
                    }
                }

                var content = $"طلب: {o.Id}\nالتاريخ: {o.OrderDate}\nالحالة: {o.Status}\nالمبلغ الكلي: {o.TotalAmount}\nتفاصيل:\n{details}";
                await UpsertDocumentEmbeddingAsync("Order", o.Id.ToString(), $"Order:{o.Id}", content);
            }
        }

        private async Task IndexOrderDetailsAsync()
        {
            var items = await _dbContext.OrderDetails.AsNoTracking().ToListAsync();
            foreach (var od in items)
            {
                var content = $"OrderDetail: Id:{od.Id}\nOrderId:{od.OrderID}\nProductId:{od.ProductID}\nPrice:{od.Price}\nQuantity:{od.Quantity}";
                await UpsertDocumentEmbeddingAsync("OrderDetail", od.Id.ToString(), $"OrderDetail:{od.Id}", content);
            }
        }

        private async Task IndexReviewsAsync()
        {
            var items = await _dbContext.Reviews.AsNoTracking().ToListAsync();
            foreach (var r in items)
            {
                var content = $"مراجعة من المستخدم: {r.UserID}\nالتعليق: {r.Comment}\nالتقييم: {r.Rating}";
                await UpsertDocumentEmbeddingAsync("Review", r.Id.ToString(), $"Review:{r.Id}", content);
            }
        }

        private async Task IndexSubscribesAsync()
        {
            var items = await _dbContext.Subscribes.AsNoTracking().ToListAsync();
            foreach (var s in items)
            {
                var content = $"اشتراك: {s.Id}\nالاسم: {s.PlanName}\nالسعر: {s.Price}";
                await UpsertDocumentEmbeddingAsync("Subscribe", s.Id.ToString(), $"Subscribe:{s.Id}", content);
            }
        }

        private async Task IndexBazarsAsync()
        {
            var items = await _dbContext.Bazars.AsNoTracking().ToListAsync();
            foreach (var b in items)
            {
                var content = $"بازار: {b.Id}\nالاسم: {b.BazarBrands}\nالوصف: {b.Title}";
                await UpsertDocumentEmbeddingAsync("Bazar", b.Id.ToString(), $"Bazar:{b.Id}", content);
            }
        }

        private async Task IndexBazarBrandsAsync()
        {
            var items = await _dbContext.BazarBrands.AsNoTracking().ToListAsync();
            foreach (var bb in items)
            {
                var content = $"BazarBrand: Id:{bb.Id}\nBazarId:{bb.BazarID}\nBrandId:{bb.BrandID}";
                await UpsertDocumentEmbeddingAsync("BazarBrand", bb.Id.ToString(), $"BazarBrand:{bb.Id}", content);
            }
        }

        private async Task IndexPaymentsAsync()
        {
            var items = await _dbContext.Payments.AsNoTracking().ToListAsync();
            foreach (var p in items)
            {
                var content = $"مدفوعات: Id:{p.Id}\nOrderId:{p.OrderID}\nالمبلغ:{p.PaymentMethod}\nالحالة:{p.PaymentStatus}";
                await UpsertDocumentEmbeddingAsync("Payment", p.Id.ToString(), $"Payment:{p.Id}", content);
            }
        }

        // --------- Upsert single DocumentEmbedding ----------
        private async Task UpsertDocumentEmbeddingAsync(string entityType, string entityId, string source, string content)
        {
            try
            {
                var existing = await _dbContext.DocumentEmbeddings
                    .FirstOrDefaultAsync(d => d.EntityType == entityType && d.EntityId == entityId);

                // generate embedding
                var resp = await _embeddingModel.EmbedContent(content);
                var vector = resp?.Embedding?.Values?.ToArray();
                if (vector == null || vector.Length == 0)
                {
                    _logger.LogWarning("Empty embedding for {Source}", source);
                    return;
                }

                var bytes = FloatArrayToBytes(vector);

                if (existing == null)
                {
                    var doc = new DocumentEmbedding
                    {
                        Id = Guid.NewGuid(),
                        EntityType = entityType,
                        EntityId = entityId,
                        Source = source,
                        Content = content,
                        Embedding = bytes,
                        Dimension = vector.Length,
                        CreatedAt = DateTime.UtcNow
                    };
                    _dbContext.DocumentEmbeddings.Add(doc);
                }
                else
                {
                    existing.Embedding = bytes;
                    existing.Dimension = vector.Length;
                    existing.UpdatedAt = DateTime.UtcNow;
                    _dbContext.DocumentEmbeddings.Update(existing);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing {EntityType}:{EntityId}", entityType, entityId);
            }
        }

        // --------- Retrieval ----------
        public async Task<List<ContextChunk>> GetTopKContextChunksAsync(string query, int k = 6)
        {
            var qResp = await _embeddingModel.EmbedContent(query);
            var qVec = qResp?.Embedding?.Values?.ToArray();
            if (qVec == null || qVec.Length == 0)
            {
                _logger.LogWarning("Query embedding empty");
                return new List<ContextChunk>();
            }

            var candidates = await _dbContext.DocumentEmbeddings
                .AsNoTracking()
                .Where(d => d.Embedding != null)
                .ToListAsync();

            var scored = new List<(DocumentEmbedding doc, float score)>();
            foreach (var c in candidates)
            {
                try
                {
                    var vec = BytesToFloatArray(c.Embedding!);
                    if (vec.Length != qVec.Length) continue;
                    var sim = CosineSimilarity(qVec, vec);
                    scored.Add((c, sim));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error computing similarity for doc {Id}", c.Id);
                }
            }

            var top = scored.OrderByDescending(x => x.score).Take(k).ToList();

            return top.Select(x => new ContextChunk
            {
                DocumentId = x.doc.Id,
                Source = x.doc.Source,
                Text = x.doc.Content,
                Score = x.score,
                Dimension = x.doc.Dimension
            }).ToList();
        }

        public async Task<string> FindMostRelevantContext(string query, int k = 6)
        {
            var chunks = await GetTopKContextChunksAsync(query, k);
            if (chunks == null || !chunks.Any()) return string.Empty;

            var sb = new StringBuilder();
            foreach (var c in chunks)
            {
                sb.AppendLine($"[Source: {c.Source}] (score: {c.Score:F3})");
                sb.AppendLine(c.Text);
                sb.AppendLine("\n---\n");
            }
            return sb.ToString();
        }

        // ---------- helpers ----------
        private static byte[] FloatArrayToBytes(float[] arr)
        {
            var bytes = new byte[arr.Length * 4];
            for (int i = 0; i < arr.Length; i++)
                Buffer.BlockCopy(BitConverter.GetBytes(arr[i]), 0, bytes, i * 4, 4);
            return bytes;
        }

        private static float[] BytesToFloatArray(byte[] bytes)
        {
            var n = bytes.Length / 4;
            var arr = new float[n];
            for (int i = 0; i < n; i++)
                arr[i] = BitConverter.ToSingle(bytes, i * 4);
            return arr;
        }

        private static float CosineSimilarity(float[] a, float[] b)
        {
            double dot = 0, na = 0, nb = 0;
            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                na += a[i] * a[i];
                nb += b[i] * b[i];
            }
            var denom = Math.Sqrt(na) * Math.Sqrt(nb) + 1e-8;
            return (float)(dot / denom);
        }
    }
}
