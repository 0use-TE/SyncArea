window.downloadFile = function (url) {
    const link = document.createElement('a');
    link.href = url;
    link.download = url.split('/').pop() || 'download.png';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};