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
            try
            {
                // 手动验证请求
                if (string.IsNullOrEmpty(request.UserId))
                {
                    return BadRequest(new { Message = "UserId 不能为空" });
                }
                if (request.WorkspaceId == Guid.Empty)
                {
                    return BadRequest(new { Message = "WorkspaceId 无效" });
                }
                if (request.Date == default)
                {
                    return BadRequest(new { Message = "Date 无效" });
                }

                var images = new List<byte[]>();
                if (request.Images != null)
                {
                    foreach (var file in request.Images)
                    {
                        if (file.Length > 0)
                        {
                            using var memoryStream = new MemoryStream();
                            await file.CopyToAsync(memoryStream);
                            images.Add(memoryStream.ToArray());
                        }
                    }
                }

                var workItem = await _workItemService.CreateWorkItemAsync(
                    request.UserId,
                    request.WorkspaceId,
                    request.Remark,
                    request.Date,
                    images.Any() ? images : null
                );

                if (workItem == null)
                {
                    return BadRequest(new { Message = "创建工作项失败" });
                }

                return Ok(workItem);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
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