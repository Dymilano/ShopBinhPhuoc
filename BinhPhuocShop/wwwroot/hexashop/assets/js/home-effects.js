// Home Page Effects - Flower Falling & Smooth Animations
(function() {
    'use strict';

    // Flower/Snowfall Effect
    function createFlowerEffect() {
        const container = document.getElementById('flower-container');
        if (!container) return;

        const flowerSymbols = ['🌸', '🌺', '🌻', '🌷', '🌹', '🌼', '💐', '✨'];
        const flowerCount = 15;
        
        for (let i = 0; i < flowerCount; i++) {
            setTimeout(() => {
                createFlower(container, flowerSymbols);
            }, i * 200);
        }
    }

    function createFlower(container, symbols) {
        const flower = document.createElement('div');
        flower.className = 'flower';
        flower.textContent = symbols[Math.floor(Math.random() * symbols.length)];
        
        const startX = Math.random() * 100;
        const endX = startX + (Math.random() * 40 - 20);
        const duration = 8 + Math.random() * 4;
        const delay = Math.random() * 2;
        const size = 20 + Math.random() * 15;
        
        flower.style.left = startX + '%';
        flower.style.fontSize = size + 'px';
        flower.style.animationDuration = duration + 's';
        flower.style.animationDelay = delay + 's';
        flower.style.opacity = 0.6 + Math.random() * 0.4;
        
        container.appendChild(flower);
        
        // Remove flower after animation
        setTimeout(() => {
            if (flower.parentNode) {
                flower.parentNode.removeChild(flower);
            }
        }, (duration + delay) * 1000);
        
        // Create new flower to maintain count
        setTimeout(() => {
            createFlower(container, symbols);
        }, (duration + delay) * 1000);
    }

    // Smooth Scroll
    function initSmoothScroll() {
        document.querySelectorAll('a[href^="#"]').forEach(anchor => {
            anchor.addEventListener('click', function(e) {
                const href = this.getAttribute('href');
                if (href === '#' || href === '#top') {
                    e.preventDefault();
                    window.scrollTo({
                        top: 0,
                        behavior: 'smooth'
                    });
                } else if (href.startsWith('#')) {
                    const target = document.querySelector(href);
                    if (target) {
                        e.preventDefault();
                        target.scrollIntoView({
                            behavior: 'smooth',
                            block: 'start'
                        });
                    }
                }
            });
        });
    }

    // Parallax Effect for Banner
    function initParallax() {
        const banner = document.querySelector('.main-banner');
        if (!banner) return;

        window.addEventListener('scroll', function() {
            const scrolled = window.pageYOffset;
            const rate = scrolled * 0.5;
            banner.style.transform = `translateY(${rate}px)`;
        });
    }

    // Initialize all effects when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            createFlowerEffect();
            initSmoothScroll();
            initParallax();
        });
    } else {
        createFlowerEffect();
        initSmoothScroll();
        initParallax();
    }

    // Re-initialize flower effect on page visibility change
    document.addEventListener('visibilitychange', function() {
        if (!document.hidden) {
            setTimeout(createFlowerEffect, 500);
        }
    });
})();
