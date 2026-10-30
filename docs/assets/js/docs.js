(function () {
  'use strict';

  var root = document.documentElement;
  var base = root.getAttribute('data-base') || '';

  function setupThemeToggle() {
    var button = document.getElementById('theme-toggle');

    function label() {
      var next = root.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
      button.setAttribute('aria-label', 'Switch to ' + next + ' theme');
    }

    label();
    button.addEventListener('click', function () {
      var next = root.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
      root.setAttribute('data-theme', next);
      try { localStorage.setItem('se-docs-theme', next); } catch (e) { }
      label();
    });
  }

  function setupMenu() {
    var button = document.getElementById('menu-toggle');
    var sidebar = document.getElementById('sidebar');

    button.addEventListener('click', function () {
      var open = sidebar.classList.toggle('open');
      button.setAttribute('aria-expanded', open ? 'true' : 'false');
      button.setAttribute('aria-label', open ? 'Hide navigation' : 'Show navigation');
    });

    var current = sidebar.querySelector('[aria-current="page"]');
    if (current) {
      var top = current.offsetTop - sidebar.clientHeight / 2;
      sidebar.scrollTop = Math.max(0, top);
    }
  }

  function setupSearch() {
    var input = document.getElementById('search-input');
    var panel = document.getElementById('search-panel');
    var status = document.getElementById('search-status');
    var list = document.getElementById('search-results');
    var pages = null;
    var loading = null;

    function load() {
      if (!loading) {
        loading = fetch(base + '/assets/search.json')
          .then(function (response) { return response.json(); })
          .then(function (data) {
            pages = data.map(function (page) {
              page.titleLower = page.title.toLowerCase();
              page.descLower = page.desc.toLowerCase();
              page.textLower = page.text.toLowerCase();
              return page;
            });
          })
          .catch(function () {
            loading = null;
            throw new Error('Search index failed to load');
          });
      }

      return loading;
    }

    function score(page, terms) {
      var total = 0;
      for (var i = 0; i < terms.length; i++) {
        var term = terms[i];
        var inTitle = page.titleLower.indexOf(term);
        var inDesc = page.descLower.indexOf(term);
        var inText = page.textLower.indexOf(term);
        if (inTitle < 0 && inDesc < 0 && inText < 0) {
          return 0;
        }

        total += inTitle === 0 ? 16 : inTitle > 0 ? 10 : 0;
        total += inDesc >= 0 ? 4 : 0;
        total += inText >= 0 ? 1 : 0;
      }

      return total;
    }

    function snippet(page, terms) {
      if (terms.some(function (term) { return page.descLower.indexOf(term) >= 0; })) {
        return page.desc;
      }

      for (var i = 0; i < terms.length; i++) {
        var at = page.textLower.indexOf(terms[i]);
        if (at >= 0) {
          var start = Math.max(0, at - 50);
          var text = page.text.substring(start, at + 90).trim();
          return (start > 0 ? '... ' : '') + text + ' ...';
        }
      }

      return page.desc;
    }

    function render() {
      var query = input.value.trim().toLowerCase();
      if (!query) {
        panel.hidden = true;
        return;
      }

      if (!pages) {
        panel.hidden = false;
        status.textContent = 'Loading...';
        list.textContent = '';
        load().then(render).catch(function () {
          status.textContent = 'Search could not be loaded.';
        });
        return;
      }

      var terms = query.split(/\s+/);
      var hits = pages
        .map(function (page) { return { page: page, score: score(page, terms) }; })
        .filter(function (hit) { return hit.score > 0; })
        .sort(function (a, b) { return b.score - a.score; })
        .slice(0, 12);

      list.textContent = '';
      hits.forEach(function (hit) {
        var link = document.createElement('a');
        link.href = base + '/' + hit.page.url;
        var group = document.createElement('small');
        group.textContent = hit.page.group;
        var title = document.createElement('strong');
        title.textContent = hit.page.title;
        var text = document.createElement('span');
        text.textContent = snippet(hit.page, terms);
        link.appendChild(group);
        link.appendChild(title);
        link.appendChild(text);
        var item = document.createElement('li');
        item.appendChild(link);
        list.appendChild(item);
      });

      status.textContent = hits.length ? hits.length + (hits.length === 1 ? ' result' : ' results') : 'No pages found';
      panel.hidden = false;
    }

    function resultLinks() {
      return Array.prototype.slice.call(list.querySelectorAll('a'));
    }

    input.addEventListener('focus', function () {
      load().catch(function () { });
      render();
    });
    input.addEventListener('input', render);

    input.addEventListener('keydown', function (e) {
      var links = resultLinks();
      if (e.key === 'ArrowDown' && links.length) {
        e.preventDefault();
        links[0].focus();
      } else if (e.key === 'Enter' && links.length) {
        e.preventDefault();
        links[0].click();
      }
    });

    list.addEventListener('keydown', function (e) {
      var links = resultLinks();
      var index = links.indexOf(document.activeElement);
      if (e.key === 'ArrowDown' && index < links.length - 1) {
        e.preventDefault();
        links[index + 1].focus();
      } else if (e.key === 'ArrowUp') {
        e.preventDefault();
        (index > 0 ? links[index - 1] : input).focus();
      }
    });

    document.addEventListener('keydown', function (e) {
      var typing = /^(INPUT|TEXTAREA|SELECT)$/.test(document.activeElement.tagName);
      if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'k' || (e.key === '/' && !typing)) {
        e.preventDefault();
        input.focus();
        input.select();
      } else if (e.key === 'Escape' && !panel.hidden) {
        panel.hidden = true;
        input.focus();
      }
    });

    document.addEventListener('click', function (e) {
      if (!document.getElementById('search').contains(e.target)) {
        panel.hidden = true;
      }
    });
  }

  function setupToc() {
    var toc = document.getElementById('toc');
    var article = document.querySelector('.prose');
    if (!toc || !article) {
      return;
    }

    var headings = Array.prototype.slice.call(article.querySelectorAll('h2[id], h3[id]'));
    if (headings.length < 2) {
      return;
    }

    var list = document.getElementById('toc-list');
    var links = headings.map(function (heading) {
      var link = document.createElement('a');
      link.href = '#' + heading.id;
      link.textContent = heading.textContent;
      var item = document.createElement('li');
      if (heading.tagName === 'H3') {
        item.className = 'toc-sub';
      }

      item.appendChild(link);
      list.appendChild(item);
      return link;
    });

    toc.hidden = false;

    var activeIndex = -1;

    function highlight() {
      var line = window.innerHeight * 0.25;
      var active = 0;
      headings.forEach(function (heading, index) {
        if (heading.getBoundingClientRect().top <= line) {
          active = index;
        }
      });

      if (active === activeIndex) {
        return;
      }

      activeIndex = active;
      links.forEach(function (link, index) {
        link.classList.toggle('active', index === active);
      });

      // Long reference pages have more headings than fit, so keep the current one in view
      var offset = links[active].offsetTop;
      if (offset < toc.scrollTop || offset > toc.scrollTop + toc.clientHeight - 40) {
        toc.scrollTop = offset - toc.clientHeight / 2;
      }
    }

    highlight();
    window.addEventListener('scroll', highlight, { passive: true });
  }

  function setupArticle() {
    var article = document.querySelector('.prose');
    if (!article) {
      return;
    }

    Array.prototype.forEach.call(article.querySelectorAll('table'), function (table) {
      var wrap = document.createElement('div');
      wrap.className = 'table-wrap';
      table.parentNode.insertBefore(wrap, table);
      wrap.appendChild(table);
    });

    // Pages open with a short "Menu:" and "Shortcut:" list before the first heading
    var element = article.firstElementChild;
    while (element && !/^H[2-6]$/.test(element.tagName)) {
      if (element.tagName === 'UL') {
        var items = Array.prototype.slice.call(element.children);
        var facts = items.every(function (item) {
          var first = item.firstElementChild;
          return first && first.tagName === 'STRONG' && /:$/.test(first.textContent.trim()) && item.textContent.indexOf(first.textContent) === 0;
        });

        if (facts) {
          element.classList.add('page-facts');
          items.forEach(function (item) {
            var label = item.firstElementChild;
            label.textContent = label.textContent.trim().replace(/:$/, '');
          });
        }

        return;
      }

      element = element.nextElementSibling;
    }
  }

  setupThemeToggle();
  setupMenu();
  setupSearch();
  setupArticle();
  setupToc();
})();
