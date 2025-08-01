using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncArea.Services;

namespace SyncArea.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkItemsController : ControllerBase
    {
        private readonly WorkItemService _workItemService;

        public WorkItemsController(WorkItemService workItemService)
        {
            _workItemService = workItemService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateWorkItem([FromForm] CreateWorkItemRequest request)
        {
            // 验证请求
            var validationResult = ValidateRequest(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { Message = validationResult.ErrorMessage });
            }

            try
            {
                var images = await ProcessImages(request.Images);
                var workItem = await _workItemService.CreateWorkItemAsync(
                    request.UserId,
                    request.WorkspaceId,
                    request.Remark,
                    request.Date,
                    images
                );

                return Ok(workItem);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // 验证请求
        private (bool IsValid, string ErrorMessage) ValidateRequest(CreateWorkItemRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId))
            {
                return (false, "UserId 不能为空");
            }

            if (request.WorkspaceId == Guid.Empty)
            {
                return (false, "WorkspaceId 无效");
            }

            if (request.Date == default)
            {
                return (false, "Date 无效");
            }

            return (true, string.Empty);
        }

        // 处理上传的图片
        private async Task<List<byte[]>?> ProcessImages(IFormFileCollection? images)
        {
            if (images == null || !images.Any())
            {
                return null;
            }

            var imageBytes = new List<byte[]>();
            foreach (var file in images)
            {
                if (file.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    await file.CopyToAsync(memoryStream);
                    imageBytes.Add(memoryStream.ToArray());
                }
            }

            return imageBytes;
        }

        public class CreateWorkItemRequest
        {
            public string? UserId { get; set; }
            public Guid WorkspaceId { get; set; }
            public string? Remark { get; set; }
            public DateTime Date { get; set; }
            public IFormFileCollection? Images { get; set; }
        }
    }
}