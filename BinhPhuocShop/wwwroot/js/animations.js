// Animations JavaScript for Bình Phước Shop

document.addEventListener('DOMContentLoaded', function() {
    // 1. Intersection Observer for scroll animations
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver(function(entries) {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.animation = 'fadeInUp 0.6s ease-out forwards';
            }
        });
    }, observerOptions);

    // Observe all isotope items
    document.querySelectorAll('.isotope-item').forEach(item => {
        observer.observe(item);
    });

    // 2. Smooth scroll for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function(e) {
            const href = this.getAttribute('href');
            if (href !== '#' && href.length > 1) {
                e.preventDefault();
                const target = document.querySelector(href);
                if (target) {
                    target.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            }
        });
    });

    // 3. Add loading animation to buttons
    document.querySelectorAll('button[type="submit"], .add-to-cart-btn').forEach(btn => {
        btn.addEventListener('click', function() {
            if (!this.disabled) {
                const originalText = this.textContent;
                this.textContent = 'Đang xử lý...';
                this.disabled = true;
                
                setTimeout(() => {
                    this.textContent = originalText;
                    this.disabled = false;
                }, 2000);
            }
        });
    });

    // 4. Animate numbers (for statistics)
    function animateValue(element, start, end, duration) {
        let startTimestamp = null;
        const step = (timestamp) => {
            if (!startTimestamp) startTimestamp = timestamp;
            const progress = Math.min((timestamp - startTimestamp) / duration, 1);
            element.textContent = Math.floor(progress * (end - start) + start);
            if (progress < 1) {
                window.requestAnimationFrame(step);
            }
        };
        window.requestAnimationFrame(step);
    }

    // 5. Parallax effect for hero section
    const heroSection = document.querySelector('.section-slide');
    if (heroSection) {
        window.addEventListener('scroll', function() {
            const scrolled = window.pageYOffset;
            if (scrolled < heroSection.offsetHeight) {
                heroSection.style.transform = `translateY(${scrolled * 0.5}px)`;
            }
        });
    }

    // 6. Stagger animation for product cards
    const productCards = document.querySelectorAll('.block2');
    productCards.forEach((card, index) => {
        card.style.animationDelay = `${index * 0.1}s`;
    });

    // 7. Image lazy loading with fade in
    const images = document.querySelectorAll('img[data-src]');
    const imageObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                img.src = img.dataset.src;
                img.classList.add('fade-in');
                imageObserver.unobserve(img);
            }
        });
    });

    images.forEach(img => imageObserver.observe(img));

    // 8. Cart count animation
    const cartCountElements = document.querySelectorAll('.qty');
    let lastCount = 0;
    
    function updateCartCount(newCount) {
        cartCountElements.forEach(el => {
            const currentCount = parseInt(el.textContent) || 0;
            if (currentCount !== newCount) {
                el.style.animation = 'pulse 0.5s ease';
                setTimeout(() => {
                    el.textContent = newCount;
                    el.style.animation = '';
                }, 250);
            }
        });
    }

    // 9. Search input focus animation
    const searchInput = document.querySelector('.plh3, input[name="q"]');
    if (searchInput) {
        searchInput.addEventListener('focus', function() {
            this.parentElement.style.transform = 'scale(1.02)';
        });
        searchInput.addEventListener('blur', function() {
            this.parentElement.style.transform = 'scale(1)';
        });
    }

    // 10. Form validation animation
    document.querySelectorAll('form').forEach(form => {
        form.addEventListener('submit', function(e) {
            const inputs = form.querySelectorAll('input[required], textarea[required]');
            let isValid = true;
            
            inputs.forEach(input => {
                if (!input.value.trim()) {
                    input.style.animation = 'shake 0.5s ease';
                    isValid = false;
                    setTimeout(() => {
                        input.style.animation = '';
                    }, 500);
                }
            });
            
            if (!isValid) {
                e.preventDefault();
            }
        });
    });

    // 11. Shake animation for errors
    const style = document.createElement('style');
    style.textContent = `
        @keyframes shake {
            0%, 100% { transform: translateX(0); }
            25% { transform: translateX(-10px); }
            75% { transform: translateX(10px); }
        }
        .fade-in {
            animation: fadeIn 0.5s ease-out;
        }
    `;
    document.head.appendChild(style);
});
