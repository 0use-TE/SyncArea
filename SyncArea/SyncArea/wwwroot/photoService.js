let selectedFiles = [];

window.initializeFileInput = (inputId, containerId, countId) => {
    selectedFiles = [];
    const input = document.getElementById(inputId);
    const container = document.getElementById(containerId);
    const countElement = document.getElementById(countId);

    input.addEventListener('change', async () => {
        const files = input.files;
        for (let i = 0; i < files.length; i++) {
            const file = files[i];
            if (file.size > 10 * 1024 * 1024) {
                alert(`图片 ${file.name} 超过 10MB`);
                continue;
            }
            if (selectedFiles.length >= 10) {
                alert('最多上传 10 张图片');
                break;
            }

            const previewUrl = await new Promise((resolve, reject) => {
                const reader = new FileReader();
                reader.onload = () => resolve(reader.result);
                reader.onerror = () => reject(new Error(`无法读取 ${file.name}`));
                reader.readAsDataURL(file);
            });

            const card = document.createElement('div');
            card.className = 'gallery-card';
            card.dataset.index = selectedFiles.length;

            const img = document.createElement('img');
            img.src = previewUrl;
            img.alt = '预览图片';
            img.style.width = '100px';
            img.style.height = '100px';
            img.style.objectFit = 'cover';

            const deleteButton = document.createElement('div');
            deleteButton.className = 'delete-button';
            deleteButton.innerHTML = '×';
            deleteButton.onclick = () => {
                card.remove();
                selectedFiles.splice(parseInt(card.dataset.index), 1);
                Array.from(container.children).forEach((child, index) => {
                    child.dataset.index = index;
                });
                countElement.textContent = `已选择 ${selectedFiles.length} 张图片`;
            };

            card.appendChild(img);
            card.appendChild(deleteButton);
            container.appendChild(card);
            selectedFiles.push(file);
        }

        countElement.textContent = `已选择 ${selectedFiles.length} 张图片`;
        input.value = '';
    });
};
window.uploadWorkItem = async (url, formData) => {
    try {
        const form = new FormData();
        console.log('上传数据:', formData);
        // 添加字段，防止空值
        form.append('UserId', formData.userId ?? '');
        form.append('WorkspaceId', formData.workspaceId ?? '');
        form.append('Remark', formData.remark ?? '');
        form.append('Date', formData.date ?? '');

        // 添加图片
        for (let i = 0; i < selectedFiles.length; i++) {
            form.append('Images', selectedFiles[i]);
        }
        console.log('选中的图片:', selectedFiles.length);

        const response = await fetch(url, {
            method: 'POST',
            body: form
        });

        const result = await response.json();
        return {
            Success: response.ok,
            Data: response.ok ? result : null,
            Error: response.ok ? '' : result?.Message || response.statusText
        };
    } catch (error) {
        return { Success: false, Data: null, Error: error.message };
    }
};

window.clearFiles = () => {
    selectedFiles = [];
    const container = document.getElementById('imagePreviewContainer');
    if (container) {
        container.innerHTML = '';
    }
    const countElement = document.getElementById('imageCount');
    if (countElement) {
        countElement.textContent = '已选择 0 张图片';
    }
};