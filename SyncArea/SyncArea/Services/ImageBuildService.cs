using Microsoft.Extensions.Options;
using SyncArea.Identity.Models;
using SyncArea.Models.Options;

namespace SyncArea.Services
{
    public class ImageBuildService
    {
        private readonly IOptions<ImagesPathModel> _imagesPathOption;
        public ImageBuildService(IOptions<ImagesPathModel> imagesPathOption)
        {
            _imagesPathOption = imagesPathOption;
        }

        public string BuildProjectNameDir(string projectName)
        {
            // 确保图片目录存在
            var imagesPath = _imagesPathOption.Value.ImagePath ?? string.Empty;
            // 构建完整的文件路径
            var directory = Path.Combine(imagesPath, projectName);
            // 确保目录存在
            return directory;
        }
        public string BuildProjectNumberDir(string projectName, string projectNumber)
        {
            var projectNameDir = BuildProjectNameDir(projectName);
            return Path.Combine(projectNameDir, projectNumber);
        }
        public string BuildImagePath(Workspace workSpace)
        {
            // 确保图片目录存在
            var imagesPath = _imagesPathOption.Value.ImagePath ?? string.Empty;

            // 构建完整的文件路径
            var date = workSpace.CreatedAt;
            var directory = Path.Combine(imagesPath, workSpace.Name, workSpace.RoomNumber, date.Year.ToString(), date.Month.ToString());
            // 确保目录存在
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return directory;
        }

        public string BuildWebImagePath(Workspace workSpace)
        {
            return string.Join("/",
                "images",
                workSpace.Name,
                workSpace.RoomNumber,
                workSpace.CreatedAt.Year.ToString(),
                workSpace.CreatedAt.Month.ToString());
        }

    }
}
