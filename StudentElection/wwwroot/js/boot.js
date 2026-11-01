try {
    if (sessionStorage.getItem('adminKey'))
        document.documentElement.classList.add('has-key');
} catch (e) { }

