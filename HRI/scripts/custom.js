!function (a) {
    a(function () {
        a.support.transition = function () {
            var a = function () {
                var a = document.createElement("bootstrap"), b = { WebkitTransition: "webkitTransitionEnd", MozTransition: "transitionend", OTransition: "oTransitionEnd otransitionend", transition: "transitionend" }, c;
                for (c in b) if (a.style[c] !== undefined) return b[c]
            }();
            return a && { end: a }
        }()
    })
}(window.jQuery), !function (a) {
    var b = function (b, c) {
        this.options = c, this.$element = a(b).delegate('[data-dismiss="modal"]', "click.dismiss.modal", a.proxy(this.hide, this)), this.options.remote && this.$element.find(".modal-body").load(this.options.remote)
    };
    b.prototype = {
        constructor: b, toggle: function () {
            return this[this.isShown ? "hide" : "show"]()
        }, show: function () {
            var b = this, c = a.Event("show");
            this.$element.trigger(c);
            if (this.isShown || c.isDefaultPrevented()) return;
            this.isShown = !0, this.escape(), this.backdrop(function () {
                var c = a.support.transition && b.$element.hasClass("fade");
                b.$element.parent().length || b.$element.appendTo(document.body), b.$element.show(), c && b.$element[0].offsetWidth, b.$element.addClass("in").attr("aria-hidden", !1), b.enforceFocus(), c ? b.$element.one(a.support.transition.end, function () {
                    b.$element.focus().trigger("shown")
                }) : b.$element.focus().trigger("shown")
            })
        }, hide: function (b) {
            b && b.preventDefault();
            var c = this;
            b = a.Event("hide"), this.$element.trigger(b);
            if (!this.isShown || b.isDefaultPrevented()) return;
            this.isShown = !1, this.escape(), a(document).off("focusin.modal"), this.$element.removeClass("in").attr("aria-hidden", !0), a.support.transition && this.$element.hasClass("fade") ? this.hideWithTransition() : this.hideModal()
        }, enforceFocus: function () {
            var b = this;
            a(document).on("focusin.modal", function (a) {
                b.$element[0] !== a.target && !b.$element.has(a.target).length && b.$element.focus()
            })
        }, escape: function () {
            var a = this;
            this.isShown && this.options.keyboard ? this.$element.on("keyup.dismiss.modal", function (b) {
                b.which == 27 && a.hide()
            }) : this.isShown || this.$element.off("keyup.dismiss.modal")
        }, hideWithTransition: function () {
            var b = this, c = setTimeout(function () {
                b.$element.off(a.support.transition.end), b.hideModal()
            }, 500);
            this.$element.one(a.support.transition.end, function () {
                clearTimeout(c), b.hideModal()
            })
        }, hideModal: function () {
            var a = this;
            this.$element.hide(), this.backdrop(function () {
                a.removeBackdrop(), a.$element.trigger("hidden")
            })
        }, removeBackdrop: function () {
            this.$backdrop && this.$backdrop.remove(), this.$backdrop = null
        }, backdrop: function (b) {
            var c = this, d = this.$element.hasClass("fade") ? "fade" : "";
            if (this.isShown && this.options.backdrop) {
                var e = a.support.transition && d;
                this.$backdrop = a('<div class="modal-backdrop ' + d + '" />').appendTo(document.body), this.$backdrop.click(this.options.backdrop == "static" ? a.proxy(this.$element[0].focus, this.$element[0]) : a.proxy(this.hide, this)), e && this.$backdrop[0].offsetWidth, this.$backdrop.addClass("in");
                if (!b) return;
                e ? this.$backdrop.one(a.support.transition.end, b) : b()
            } else !this.isShown && this.$backdrop ? (this.$backdrop.removeClass("in"), a.support.transition && this.$element.hasClass("fade") ? this.$backdrop.one(a.support.transition.end, b) : b()) : b && b()
        }
    };
    var c = a.fn.modal;
    a.fn.modal = function (c) {
        return this.each(function () {
            var d = a(this), e = d.data("modal"), f = a.extend({}, a.fn.modal.defaults, d.data(), typeof c == "object" && c);
            e || d.data("modal", e = new b(this, f)), typeof c == "string" ? e[c]() : f.show && e.show()
        })
    }, a.fn.modal.defaults = { backdrop: !0, keyboard: !0, show: !0 }, a.fn.modal.Constructor = b, a.fn.modal.noConflict = function () {
        return a.fn.modal = c, this
    }, a(document).on("click.modal.data-api", '[data-toggle="modal"]', function (b) {
        var c = a(this), d = c.attr("href"), e = a(c.attr("data-target") || d && d.replace(/.*(?=#[^\s]+$)/, "")), f = e.data("modal") ? "toggle" : a.extend({ remote: !/#/.test(d) && d }, e.data(), c.data());
        b.preventDefault(), e.modal(f).one("hide", function () {
            c.focus()
        })
    })
}(window.jQuery), !function (a) {
    function b() {
        a(".dropdown-backdrop").remove(), a(d).each(function () {
            c(a(this)).removeClass("open")
        })
    }

    function c(b) {
        var c = b.attr("data-target"), d;
        c || (c = b.attr("href"), c = c && /#/.test(c) && c.replace(/.*(?=#[^\s]*$)/, "")), d = c && a(c);
        if (!d || !d.length) d = b.parent();
        return d
    }

    var d = "[data-toggle=dropdown]", e = function (b) {
        var c = a(b).on("click.dropdown.data-api", this.toggle);
        a("html").on("click.dropdown.data-api", function () {
            c.parent().removeClass("open")
        })
    };
    e.prototype = {
        constructor: e, toggle: function (d) {
            var e = a(this), f, g;
            if (e.is(".disabled, :disabled")) return;
            return f = c(e), g = f.hasClass("open"), b(), g || ("ontouchstart" in document.documentElement && a('<div class="dropdown-backdrop"/>').insertBefore(a(this)).on("click", b), f.toggleClass("open")), e.focus(), !1
        }, keydown: function (b) {
            var e, f, g, h, i, j;
            if (!/(38|40|27)/.test(b.keyCode)) return;
            e = a(this), b.preventDefault(), b.stopPropagation();
            if (e.is(".disabled, :disabled")) return;
            h = c(e), i = h.hasClass("open");
            if (!i || i && b.keyCode == 27) return b.which == 27 && h.find(d).focus(), e.click();
            f = a("[role=menu] li:not(.divider):visible a", h);
            if (!f.length) return;
            j = f.index(f.filter(":focus")), b.keyCode == 38 && j > 0 && j--, b.keyCode == 40 && j < f.length - 1 && j++, ~j || (j = 0), f.eq(j).focus()
        }
    };
    var f = a.fn.dropdown;
    a.fn.dropdown = function (b) {
        return this.each(function () {
            var c = a(this), d = c.data("dropdown");
            d || c.data("dropdown", d = new e(this)), typeof b == "string" && d[b].call(c)
        })
    }, a.fn.dropdown.Constructor = e, a.fn.dropdown.noConflict = function () {
        return a.fn.dropdown = f, this
    }, a(document).on("click.dropdown.data-api", b).on("click.dropdown.data-api", ".dropdown form", function (a) {
        a.stopPropagation()
    }).on("click.dropdown.data-api", d, e.prototype.toggle).on("keydown.dropdown.data-api", d + ", [role=menu]", e.prototype.keydown)
}(window.jQuery), !function (a) {
    function b(b, c) {
        var d = a.proxy(this.process, this), e = a(b).is("body") ? a(window) : a(b), f;
        this.options = a.extend({}, a.fn.scrollspy.defaults, c), this.$scrollElement = e.on("scroll.scroll-spy.data-api", d), this.selector = (this.options.target || (f = a(b).attr("href")) && f.replace(/.*(?=#[^\s]+$)/, "") || "") + " .nav li > a", this.$body = a("body"), this.refresh(), this.process()
    }

    b.prototype = {
        constructor: b, refresh: function () {
            var b = this, c;
            this.offsets = a([]), this.targets = a([]), c = this.$body.find(this.selector).map(function () {
                var c = a(this), d = c.data("target") || c.attr("href"), e = /^#\w/.test(d) && a(d);
                return e && e.length && [
                    [e.position().top + (!a.isWindow(b.$scrollElement.get(0)) && b.$scrollElement.scrollTop()), d]
                ] || null
            }).sort(function (a, b) {
                return a[0] - b[0]
            }).each(function () {
                b.offsets.push(this[0]), b.targets.push(this[1])
            })
        }, process: function () {
            var a = this.$scrollElement.scrollTop() + this.options.offset, b = this.$scrollElement[0].scrollHeight || this.$body[0].scrollHeight, c = b - this.$scrollElement.height(), d = this.offsets, e = this.targets, f = this.activeTarget, g;
            if (a >= c) return f != (g = e.last()[0]) && this.activate(g);
            for (g = d.length; g--;) f != e[g] && a >= d[g] && (!d[g + 1] || a <= d[g + 1]) && this.activate(e[g])
        }, activate: function (b) {
            var c, d;
            this.activeTarget = b, a(this.selector).parent(".active").removeClass("active"), d = this.selector + '[data-target="' + b + '"],' + this.selector + '[href="' + b + '"]', c = a(d).parent("li").addClass("active"), c.parent(".dropdown-menu").length && (c = c.closest("li.dropdown").addClass("active")), c.trigger("activate")
        }
    };
    var c = a.fn.scrollspy;
    a.fn.scrollspy = function (c) {
        return this.each(function () {
            var d = a(this), e = d.data("scrollspy"), f = typeof c == "object" && c;
            e || d.data("scrollspy", e = new b(this, f)), typeof c == "string" && e[c]()
        })
    }, a.fn.scrollspy.Constructor = b, a.fn.scrollspy.defaults = { offset: 10 }, a.fn.scrollspy.noConflict = function () {
        return a.fn.scrollspy = c, this
    }, a(window).on("load", function () {
        a('[data-spy="scroll"]').each(function () {
            var b = a(this);
            b.scrollspy(b.data())
        })
    })
}(window.jQuery), !function (a) {
    var b = function (b) {
        this.element = a(b)
    };
    b.prototype = {
        constructor: b, show: function () {
            var b = this.element, c = b.closest("ul:not(.dropdown-menu)"), d = b.attr("data-target"), e, f, g;
            d || (d = b.attr("href"), d = d && d.replace(/.*(?=#[^\s]*$)/, ""));
            if (b.parent("li").hasClass("active")) return;
            e = c.find(".active:last a")[0], g = a.Event("show", { relatedTarget: e }), b.trigger(g);
            if (g.isDefaultPrevented()) return;
            f = a(d), this.activate(b.parent("li"), c), this.activate(f, f.parent(), function () {
                b.trigger({ type: "shown", relatedTarget: e })
            })
        }, activate: function (b, c, d) {
            function e() {
                f.removeClass("active").find("> .dropdown-menu > .active").removeClass("active"), b.addClass("active"), g ? (b[0].offsetWidth, b.addClass("in")) : b.removeClass("fade"), b.parent(".dropdown-menu") && b.closest("li.dropdown").addClass("active"), d && d()
            }

            var f = c.find("> .active"), g = d && a.support.transition && f.hasClass("fade");
            g ? f.one(a.support.transition.end, e) : e(), f.removeClass("in")
        }
    };
    var c = a.fn.tab;
    a.fn.tab = function (c) {
        return this.each(function () {
            var d = a(this), e = d.data("tab");
            e || d.data("tab", e = new b(this)), typeof c == "string" && e[c]()
        })
    }, a.fn.tab.Constructor = b, a.fn.tab.noConflict = function () {
        return a.fn.tab = c, this
    }, a(document).on("click.tab.data-api", '[data-toggle="tab"], [data-toggle="pill"]', function (b) {
        b.preventDefault(), a(this).tab("show")
    })
}(window.jQuery), !function (a) {
    var b = function (a, b) {
        this.init("tooltip", a, b)
    };
    b.prototype = {
        constructor: b, init: function (b, c, d) {
            var e, f, g, h, i;
            this.type = b, this.$element = a(c), this.options = this.getOptions(d), this.enabled = !0, g = this.options.trigger.split(" ");
            for (i = g.length; i--;) h = g[i], h == "click" ? this.$element.on("click." + this.type, this.options.selector, a.proxy(this.toggle, this)) : h != "manual" && (e = h == "hover" ? "mouseenter" : "focus", f = h == "hover" ? "mouseleave" : "blur", this.$element.on(e + "." + this.type, this.options.selector, a.proxy(this.enter, this)), this.$element.on(f + "." + this.type, this.options.selector, a.proxy(this.leave, this)));
            this.options.selector ? this._options = a.extend({}, this.options, { trigger: "manual", selector: "" }) : this.fixTitle()
        }, getOptions: function (b) {
            return b = a.extend({}, a.fn[this.type].defaults, this.$element.data(), b), b.delay && typeof b.delay == "number" && (b.delay = { show: b.delay, hide: b.delay }), b
        }, enter: function (b) {
            var c = a.fn[this.type].defaults, d = {}, e;
            this._options && a.each(this._options, function (a, b) {
                c[a] != b && (d[a] = b)
            }, this), e = a(b.currentTarget)[this.type](d).data(this.type);
            if (!e.options.delay || !e.options.delay.show) return e.show();
            clearTimeout(this.timeout), e.hoverState = "in", this.timeout = setTimeout(function () {
                e.hoverState == "in" && e.show()
            }, e.options.delay.show)
        }, leave: function (b) {
            var c = a(b.currentTarget)[this.type](this._options).data(this.type);
            this.timeout && clearTimeout(this.timeout);
            if (!c.options.delay || !c.options.delay.hide) return c.hide();
            c.hoverState = "out", this.timeout = setTimeout(function () {
                c.hoverState == "out" && c.hide()
            }, c.options.delay.hide)
        }, show: function () {
            var b, c, d, e, f, g, h = a.Event("show");
            if (this.hasContent() && this.enabled) {
                this.$element.trigger(h);
                if (h.isDefaultPrevented()) return;
                b = this.tip(), this.setContent(), this.options.animation && b.addClass("fade"), f = typeof this.options.placement == "function" ? this.options.placement.call(this, b[0], this.$element[0]) : this.options.placement, b.detach().css({ top: 0, left: 0, display: "block" }), this.options.container ? b.appendTo(this.options.container) : b.insertAfter(this.$element), c = this.getPosition(), d = b[0].offsetWidth, e = b[0].offsetHeight;
                switch (f) {
                    case "bottom":
                        g = { top: c.top + c.height, left: c.left + c.width / 2 - d / 2 };
                        break;
                    case "top":
                        g = { top: c.top - e, left: c.left + c.width / 2 - d / 2 };
                        break;
                    case "left":
                        g = { top: c.top + c.height / 2 - e / 2, left: c.left - d };
                        break;
                    case "right":
                        g = { top: c.top + c.height / 2 - e / 2, left: c.left + c.width }
                }
                this.applyPlacement(g, f), this.$element.trigger("shown")
            }
        }, applyPlacement: function (a, b) {
            var c = this.tip(), d = c[0].offsetWidth, e = c[0].offsetHeight, f, g, h, i;
            c.offset(a).addClass(b).addClass("in"), f = c[0].offsetWidth, g = c[0].offsetHeight, b == "top" && g != e && (a.top = a.top + e - g, i = !0), b == "bottom" || b == "top" ? (h = 0, a.left < 0 && (h = a.left * -2, a.left = 0, c.offset(a), f = c[0].offsetWidth, g = c[0].offsetHeight), this.replaceArrow(h - d + f, f, "left")) : this.replaceArrow(g - e, g, "top"), i && c.offset(a)
        }, replaceArrow: function (a, b, c) {
            this.arrow().css(c, a ? 50 * (1 - a / b) + "%" : "")
        }, setContent: function () {
            var a = this.tip(), b = this.getTitle();
            a.find(".tooltip-inner")[this.options.html ? "html" : "text"](b), a.removeClass("fade in top bottom left right")
        }, hide: function () {
            function b() {
                var b = setTimeout(function () {
                    d.off(a.support.transition.end).detach()
                }, 500);
                d.one(a.support.transition.end, function () {
                    clearTimeout(b), d.detach()
                })
            }

            var c = this, d = this.tip(), e = a.Event("hide");
            this.$element.trigger(e);
            if (e.isDefaultPrevented()) return;
            return d.removeClass("in"), a.support.transition && this.$tip.hasClass("fade") ? b() : d.detach(), this.$element.trigger("hidden"), this
        }, fixTitle: function () {
            var a = this.$element;
            (a.attr("title") || typeof a.attr("data-original-title") != "string") && a.attr("data-original-title", a.attr("title") || "").attr("title", "")
        }, hasContent: function () {
            return this.getTitle()
        }, getPosition: function () {
            var b = this.$element[0];
            return a.extend({}, typeof b.getBoundingClientRect == "function" ? b.getBoundingClientRect() : { width: b.offsetWidth, height: b.offsetHeight }, this.$element.offset())
        }, getTitle: function () {
            var a, b = this.$element, c = this.options;
            return a = b.attr("data-original-title") || (typeof c.title == "function" ? c.title.call(b[0]) : c.title), a
        }, tip: function () {
            return this.$tip = this.$tip || a(this.options.template)
        }, arrow: function () {
            return this.$arrow = this.$arrow || this.tip().find(".tooltip-arrow")
        }, validate: function () {
            this.$element[0].parentNode || (this.hide(), this.$element = null, this.options = null)
        }, enable: function () {
            this.enabled = !0
        }, disable: function () {
            this.enabled = !1
        }, toggleEnabled: function () {
            this.enabled = !this.enabled
        }, toggle: function (b) {
            var c = b ? a(b.currentTarget)[this.type](this._options).data(this.type) : this;
            c.tip().hasClass("in") ? c.hide() : c.show()
        }, destroy: function () {
            this.hide().$element.off("." + this.type).removeData(this.type)
        }
    };
    var c = a.fn.tooltip;
    a.fn.tooltip = function (c) {
        return this.each(function () {
            var d = a(this), e = d.data("tooltip"), f = typeof c == "object" && c;
            e || d.data("tooltip", e = new b(this, f)), typeof c == "string" && e[c]()
        })
    }, a.fn.tooltip.Constructor = b, a.fn.tooltip.defaults = { animation: !0, placement: "top", selector: !1, template: '<div class="tooltip"><div class="tooltip-arrow"></div><div class="tooltip-inner"></div></div>', trigger: "hover focus", title: "", delay: 0, html: !1, container: !1 }, a.fn.tooltip.noConflict = function () {
        return a.fn.tooltip = c, this
    }
}(window.jQuery), !function (a) {
    var b = function (a, b) {
        this.init("popover", a, b)
    };
    b.prototype = a.extend({}, a.fn.tooltip.Constructor.prototype, {
        constructor: b, setContent: function () {
            var a = this.tip(), b = this.getTitle(), c = this.getContent();
            a.find(".popover-title")[this.options.html ? "html" : "text"](b), a.find(".popover-content")[this.options.html ? "html" : "text"](c), a.removeClass("fade top bottom left right in")
        }, hasContent: function () {
            return this.getTitle() || this.getContent()
        }, getContent: function () {
            var a, b = this.$element, c = this.options;
            return a = (typeof c.content == "function" ? c.content.call(b[0]) : c.content) || b.attr("data-content"), a
        }, tip: function () {
            return this.$tip || (this.$tip = a(this.options.template)), this.$tip
        }, destroy: function () {
            this.hide().$element.off("." + this.type).removeData(this.type)
        }
    });
    var c = a.fn.popover;
    a.fn.popover = function (c) {
        return this.each(function () {
            var d = a(this), e = d.data("popover"), f = typeof c == "object" && c;
            e || d.data("popover", e = new b(this, f)), typeof c == "string" && e[c]()
        })
    }, a.fn.popover.Constructor = b, a.fn.popover.defaults = a.extend({}, a.fn.tooltip.defaults, { placement: "right", trigger: "click", content: "", template: '<div class="popover"><div class="arrow"></div><h3 class="popover-title"></h3><div class="popover-content"></div></div>' }), a.fn.popover.noConflict = function () {
        return a.fn.popover = c, this
    }
}(window.jQuery), !function (a) {
    var b = function (b, c) {
        this.options = a.extend({}, a.fn.affix.defaults, c), this.$window = a(window).on("scroll.affix.data-api", a.proxy(this.checkPosition, this)).on("click.affix.data-api", a.proxy(function () {
            setTimeout(a.proxy(this.checkPosition, this), 1)
        }, this)), this.$element = a(b), this.checkPosition()
    };
    b.prototype.checkPosition = function () {
        if (!this.$element.is(":visible")) return;
        var b = a(document).height(), c = this.$window.scrollTop(), d = this.$element.offset(), e = this.options.offset, f = e.bottom, g = e.top, h = "affix affix-top affix-bottom", i;
        typeof e != "object" && (f = g = e), typeof g == "function" && (g = e.top()), typeof f == "function" && (f = e.bottom()), i = this.unpin != null && c + this.unpin <= d.top ? !1 : f != null && d.top + this.$element.height() >= b - f ? "bottom" : g != null && c <= g ? "top" : !1;
        if (this.affixed === i) return;
        this.affixed = i, this.unpin = i == "bottom" ? d.top - c : null, this.$element.removeClass(h).addClass("affix" + (i ? "-" + i : ""))
    };
    var c = a.fn.affix;
    a.fn.affix = function (c) {
        return this.each(function () {
            var d = a(this), e = d.data("affix"), f = typeof c == "object" && c;
            e || d.data("affix", e = new b(this, f)), typeof c == "string" && e[c]()
        })
    }, a.fn.affix.Constructor = b, a.fn.affix.defaults = { offset: 0 }, a.fn.affix.noConflict = function () {
        return a.fn.affix = c, this
    }, a(window).on("load", function () {
        a('[data-spy="affix"]').each(function () {
            var b = a(this), c = b.data();
            c.offset = c.offset || {}, c.offsetBottom && (c.offset.bottom = c.offsetBottom), c.offsetTop && (c.offset.top = c.offsetTop), b.affix(c)
        })
    })
}(window.jQuery), !function (a) {
    var b = '[data-dismiss="alert"]', c = function (c) {
        a(c).on("click", b, this.close)
    };
    c.prototype.close = function (b) {
        function c() {
            f.trigger("closed").remove()
        }

        var d = a(this), e = d.attr("data-target"), f;
        e || (e = d.attr("href"), e = e && e.replace(/.*(?=#[^\s]*$)/, "")), f = a(e), b && b.preventDefault(), f.length || (f = d.hasClass("alert") ? d : d.parent()), f.trigger(b = a.Event("close"));
        if (b.isDefaultPrevented()) return;
        f.removeClass("in"), a.support.transition && f.hasClass("fade") ? f.on(a.support.transition.end, c) : c()
    };
    var d = a.fn.alert;
    a.fn.alert = function (b) {
        return this.each(function () {
            var d = a(this), e = d.data("alert");
            e || d.data("alert", e = new c(this)), typeof b == "string" && e[b].call(d)
        })
    }, a.fn.alert.Constructor = c, a.fn.alert.noConflict = function () {
        return a.fn.alert = d, this
    }, a(document).on("click.alert.data-api", b, c.prototype.close)
}(window.jQuery), !function (a) {
    var b = function (b, c) {
        this.$element = a(b), this.options = a.extend({}, a.fn.button.defaults, c)
    };
    b.prototype.setState = function (a) {
        var b = "disabled", c = this.$element, d = c.data(), e = c.is("input") ? "val" : "html";
        a += "Text", d.resetText || c.data("resetText", c[e]()), c[e](d[a] || this.options[a]), setTimeout(function () {
            a == "loadingText" ? c.addClass(b).attr(b, b) : c.removeClass(b).removeAttr(b)
        }, 0)
    }, b.prototype.toggle = function () {
        var a = this.$element.closest('[data-toggle="buttons-radio"]');
        a && a.find(".active").removeClass("active"), this.$element.toggleClass("active")
    };
    var c = a.fn.button;
    a.fn.button = function (c) {
        return this.each(function () {
            var d = a(this), e = d.data("button"), f = typeof c == "object" && c;
            e || d.data("button", e = new b(this, f)), c == "toggle" ? e.toggle() : c && e.setState(c)
        })
    }, a.fn.button.defaults = { loadingText: "loading..." }, a.fn.button.Constructor = b, a.fn.button.noConflict = function () {
        return a.fn.button = c, this
    }, a(document).on("click.button.data-api", "[data-toggle^=button]", function (b) {
        var c = a(b.target);
        c.hasClass("btn") || (c = c.closest(".btn")), c.button("toggle")
    })
}(window.jQuery), !function (a) {
    var b = function (b, c) {
        this.$element = a(b), this.options = a.extend({}, a.fn.collapse.defaults, c), this.options.parent && (this.$parent = a(this.options.parent)), this.options.toggle && this.toggle()
    };
    b.prototype = {
        constructor: b, dimension: function () {
            var a = this.$element.hasClass("width");
            return a ? "width" : "height"
        }, show: function () {
            var b, c, d, e;
            if (this.transitioning || this.$element.hasClass("in")) return;
            b = this.dimension(), c = a.camelCase(["scroll", b].join("-")), d = this.$parent && this.$parent.find("> .accordion-group > .in");
            if (d && d.length) {
                e = d.data("collapse");
                if (e && e.transitioning) return;
                d.collapse("hide"), e || d.data("collapse", null)
            }
            this.$element[b](0), this.transition("addClass", a.Event("show"), "shown"), a.support.transition && this.$element[b](this.$element[0][c])
        }, hide: function () {
            var b;
            if (this.transitioning || !this.$element.hasClass("in")) return;
            b = this.dimension(), this.reset(this.$element[b]()), this.transition("removeClass", a.Event("hide"), "hidden"), this.$element[b](0)
        }, reset: function (a) {
            var b = this.dimension();
            return this.$element.removeClass("collapse")[b](a || "auto")[0].offsetWidth, this.$element[a !== null ? "addClass" : "removeClass"]("collapse"), this
        }, transition: function (b, c, d) {
            var e = this, f = function () {
                c.type == "show" && e.reset(), e.transitioning = 0, e.$element.trigger(d)
            };
            this.$element.trigger(c);
            if (c.isDefaultPrevented()) return;
            this.transitioning = 1, this.$element[b]("in"), a.support.transition && this.$element.hasClass("collapse") ? this.$element.one(a.support.transition.end, f) : f()
        }, toggle: function () {
            this[this.$element.hasClass("in") ? "hide" : "show"]()
        }
    };
    var c = a.fn.collapse;
    a.fn.collapse = function (c) {
        return this.each(function () {
            var d = a(this), e = d.data("collapse"), f = a.extend({}, a.fn.collapse.defaults, d.data(), typeof c == "object" && c);
            e || d.data("collapse", e = new b(this, f)), typeof c == "string" && e[c]()
        })
    }, a.fn.collapse.defaults = { toggle: !0 }, a.fn.collapse.Constructor = b, a.fn.collapse.noConflict = function () {
        return a.fn.collapse = c, this
    }, a(document).on("click.collapse.data-api", "[data-toggle=collapse]", function (b) {
        var c = a(this), d, e = c.attr("data-target") || b.preventDefault() || (d = c.attr("href")) && d.replace(/.*(?=#[^\s]+$)/, ""), f = a(e).data("collapse") ? "toggle" : c.data();
        c[a(e).hasClass("in") ? "addClass" : "removeClass"]("collapsed"), a(e).collapse(f)
    })
}(window.jQuery), !function (a) {
    var b = function (b, c) {
        this.$element = a(b), this.$indicators = this.$element.find(".carousel-indicators"), this.options = c, this.options.pause == "hover" && this.$element.on("mouseenter", a.proxy(this.pause, this)).on("mouseleave", a.proxy(this.cycle, this))
    };
    b.prototype = {
        cycle: function (b) {
            return b || (this.paused = !1), this.interval && clearInterval(this.interval), this.options.interval && !this.paused && (this.interval = setInterval(a.proxy(this.next, this), this.options.interval)), this
        }, getActiveIndex: function () {
            return this.$active = this.$element.find(".item.active"), this.$items = this.$active.parent().children(), this.$items.index(this.$active)
        }, to: function (b) {
            var c = this.getActiveIndex(), d = this;
            if (b > this.$items.length - 1 || b < 0) return;
            return this.sliding ? this.$element.one("slid", function () {
                d.to(b)
            }) : c == b ? this.pause().cycle() : this.slide(b > c ? "next" : "prev", a(this.$items[b]))
        }, pause: function (b) {
            return b || (this.paused = !0), this.$element.find(".next, .prev").length && a.support.transition.end && (this.$element.trigger(a.support.transition.end), this.cycle(!0)), clearInterval(this.interval), this.interval = null, this
        }, next: function () {
            if (this.sliding) return;
            return this.slide("next")
        }, prev: function () {
            if (this.sliding) return;
            return this.slide("prev")
        }, slide: function (b, c) {
            var d = this.$element.find(".item.active"), e = c || d[b](), f = this.interval, g = b == "next" ? "left" : "right", h = b == "next" ? "first" : "last", i = this, j;
            this.sliding = !0, f && this.pause(), e = e.length ? e : this.$element.find(".item")[h](), j = a.Event("slide", { relatedTarget: e[0], direction: g });
            if (e.hasClass("active")) return;
            this.$indicators.length && (this.$indicators.find(".active").removeClass("active"), this.$element.one("slid", function () {
                var b = a(i.$indicators.children()[i.getActiveIndex()]);
                b && b.addClass("active")
            }));
            if (a.support.transition && this.$element.hasClass("slide")) {
                this.$element.trigger(j);
                if (j.isDefaultPrevented()) return;
                e.addClass(b), e[0].offsetWidth, d.addClass(g), e.addClass(g), this.$element.one(a.support.transition.end, function () {
                    e.removeClass([b, g].join(" ")).addClass("active"), d.removeClass(["active", g].join(" ")), i.sliding = !1, setTimeout(function () {
                        i.$element.trigger("slid")
                    }, 0)
                })
            } else {
                this.$element.trigger(j);
                if (j.isDefaultPrevented()) return;
                d.removeClass("active"), e.addClass("active"), this.sliding = !1, this.$element.trigger("slid")
            }
            return f && this.cycle(), this
        }
    };
    var c = a.fn.carousel;
    a.fn.carousel = function (c) {
        return this.each(function () {
            var d = a(this), e = d.data("carousel"), f = a.extend({}, a.fn.carousel.defaults, typeof c == "object" && c), g = typeof c == "string" ? c : f.slide;
            e || d.data("carousel", e = new b(this, f)), typeof c == "number" ? e.to(c) : g ? e[g]() : f.interval && e.pause().cycle()
        })
    }, a.fn.carousel.defaults = { interval: 5e3, pause: "hover" }, a.fn.carousel.Constructor = b, a.fn.carousel.noConflict = function () {
        return a.fn.carousel = c, this
    }, a(document).on("click.carousel.data-api", "[data-slide], [data-slide-to]", function (b) {
        var c = a(this), d, e = a(c.attr("data-target") || (d = c.attr("href")) && d.replace(/.*(?=#[^\s]+$)/, "")), f = a.extend({}, e.data(), c.data()), g;
        e.carousel(f), (g = c.attr("data-slide-to")) && e.data("carousel").pause().to(g).cycle(), b.preventDefault()
    })
}(window.jQuery), !function (a) {
    var b = function (b, c) {
        this.$element = a(b), this.options = a.extend({}, a.fn.typeahead.defaults, c), this.matcher = this.options.matcher || this.matcher, this.sorter = this.options.sorter || this.sorter, this.highlighter = this.options.highlighter || this.highlighter, this.updater = this.options.updater || this.updater, this.source = this.options.source, this.$menu = a(this.options.menu), this.shown = !1, this.listen()
    };
    b.prototype = {
        constructor: b, select: function () {
            var a = this.$menu.find(".active").attr("data-value");
            return this.$element.val(this.updater(a)).change(), this.hide()
        }, updater: function (a) {
            return a
        }, show: function () {
            var b = a.extend({}, this.$element.position(), { height: this.$element[0].offsetHeight });
            return this.$menu.insertAfter(this.$element).css({ top: b.top + b.height, left: b.left }).show(), this.shown = !0, this
        }, hide: function () {
            return this.$menu.hide(), this.shown = !1, this
        }, lookup: function (b) {
            var c;
            return this.query = this.$element.val(), !this.query || this.query.length < this.options.minLength ? this.shown ? this.hide() : this : (c = a.isFunction(this.source) ? this.source(this.query, a.proxy(this.process, this)) : this.source, c ? this.process(c) : this)
        }, process: function (b) {
            var c = this;
            return b = a.grep(b, function (a) {
                return c.matcher(a)
            }), b = this.sorter(b), b.length ? this.render(b.slice(0, this.options.items)).show() : this.shown ? this.hide() : this
        }, matcher: function (a) {
            return ~a.toLowerCase().indexOf(this.query.toLowerCase())
        }, sorter: function (a) {
            var b = [], c = [], d = [], e;
            while (e = a.shift()) e.toLowerCase().indexOf(this.query.toLowerCase()) ? ~e.indexOf(this.query) ? c.push(e) : d.push(e) : b.push(e);
            return b.concat(c, d)
        }, highlighter: function (a) {
            var b = this.query.replace(/[\-\[\]{}()*+?.,\\\^$|#\s]/g, "\\$&");
            return a.replace(new RegExp("(" + b + ")", "ig"), function (a, b) {
                return "<strong>" + b + "</strong>"
            })
        }, render: function (b) {
            var c = this;
            return b = a(b).map(function (b, d) {
                return b = a(c.options.item).attr("data-value", d), b.find("a").html(c.highlighter(d)), b[0]
            }), b.first().addClass("active"), this.$menu.html(b), this
        }, next: function (b) {
            var c = this.$menu.find(".active").removeClass("active"), d = c.next();
            d.length || (d = a(this.$menu.find("li")[0])), d.addClass("active")
        }, prev: function (a) {
            var b = this.$menu.find(".active").removeClass("active"), c = b.prev();
            c.length || (c = this.$menu.find("li").last()), c.addClass("active")
        }, listen: function () {
            this.$element.on("focus", a.proxy(this.focus, this)).on("blur", a.proxy(this.blur, this)).on("keypress", a.proxy(this.keypress, this)).on("keyup", a.proxy(this.keyup, this)), this.eventSupported("keydown") && this.$element.on("keydown", a.proxy(this.keydown, this)), this.$menu.on("click", a.proxy(this.click, this)).on("mouseenter", "li", a.proxy(this.mouseenter, this)).on("mouseleave", "li", a.proxy(this.mouseleave, this))
        }, eventSupported: function (a) {
            var b = a in this.$element;
            return b || (this.$element.setAttribute(a, "return;"), b = typeof this.$element[a] == "function"), b
        }, move: function (a) {
            if (!this.shown) return;
            switch (a.keyCode) {
                case 9:
                case 13:
                case 27:
                    a.preventDefault();
                    break;
                case 38:
                    a.preventDefault(), this.prev();
                    break;
                case 40:
                    a.preventDefault(), this.next()
            }
            a.stopPropagation()
        }, keydown: function (b) {
            this.suppressKeyPressRepeat = ~a.inArray(b.keyCode, [40, 38, 9, 13, 27]), this.move(b)
        }, keypress: function (a) {
            if (this.suppressKeyPressRepeat) return;
            this.move(a)
        }, keyup: function (a) {
            switch (a.keyCode) {
                case 40:
                case 38:
                case 16:
                case 17:
                case 18:
                    break;
                case 9:
                case 13:
                    if (!this.shown) return;
                    this.select();
                    break;
                case 27:
                    if (!this.shown) return;
                    this.hide();
                    break;
                default:
                    this.lookup()
            }
            a.stopPropagation(), a.preventDefault()
        }, focus: function (a) {
            this.focused = !0
        }, blur: function (a) {
            this.focused = !1, !this.mousedover && this.shown && this.hide()
        }, click: function (a) {
            a.stopPropagation(), a.preventDefault(), this.select(), this.$element.focus()
        }, mouseenter: function (b) {
            this.mousedover = !0, this.$menu.find(".active").removeClass("active"), a(b.currentTarget).addClass("active")
        }, mouseleave: function (a) {
            this.mousedover = !1, !this.focused && this.shown && this.hide()
        }
    };
    var c = a.fn.typeahead;
    a.fn.typeahead = function (c) {
        return this.each(function () {
            var d = a(this), e = d.data("typeahead"), f = typeof c == "object" && c;
            e || d.data("typeahead", e = new b(this, f)), typeof c == "string" && e[c]()
        })
    }, a.fn.typeahead.defaults = { source: [], items: 8, menu: '<ul class="typeahead dropdown-menu"></ul>', item: '<li><a href="#"></a></li>', minLength: 1 }, a.fn.typeahead.Constructor = b, a.fn.typeahead.noConflict = function () {
        return a.fn.typeahead = c, this
    }, a(document).on("focus.typeahead.data-api", '[data-provide="typeahead"]', function (b) {
        var c = a(this);
        if (c.data("typeahead")) return;
        c.typeahead(c.data())
    })
}(window.jQuery);
!function (a) {
    "use strict";
    var b = function (a) {
        this.messages = { defaultMessage: "This value seems to be invalid.", type: { email: "This value should be a valid email.", url: "This value should be a valid url.", urlstrict: "This value should be a valid url.", number: "This value should be a valid number.", digits: "This value should be digits.", dateIso: "This value should be a valid date (YYYY-MM-DD).", alphanum: "This value should be alphanumeric.", phone: "This value should be a valid phone number." }, notnull: "This value should not be null.", notblank: "This value should not be blank.", required: "This value is required.", regexp: "This value seems to be invalid.", min: "This value should be greater than or equal to %s.", max: "This value should be lower than or equal to %s.", range: "This value should be between %s and %s.", minlength: "This value is too short. It should have %s characters or more.", maxlength: "This value is too long. It should have %s characters or less.", rangelength: "This value length is invalid. It should be between %s and %s characters long.", mincheck: "You must select at least %s choices.", maxcheck: "You must select %s choices or less.", rangecheck: "You must select between %s and %s choices.", equalto: "This value should be the same." }, this.init(a)
    };
    b.prototype = {
        constructor: b, validators: {
            notnull: function (a) {
                return a.length > 0
            }, notblank: function (a) {
                return "string" === typeof a && "" !== a.replace(/^\s+/g, "").replace(/\s+$/g, "")
            }, required: function (a) {
                if ("object" === typeof a) {
                    for (var b in a) {
                        if (this.required(a[b])) {
                            return true
                        }
                    }
                    return false
                }
                return this.notnull(a) && this.notblank(a)
            }, type: function (a, b) {
                var c;
                switch (b) {
                    case "number":
                        c = /^-?(?:\d+|\d{1,3}(?:,\d{3})+)?(?:\.\d+)?$/;
                        break;
                    case "digits":
                        c = /^\d+$/;
                        break;
                    case "alphanum":
                        c = /^\w+$/;
                        break;
                    case "email":
                        c = /^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))$/i;
                        break;
                    case "url":
                        a = new RegExp("(https?|s?ftp|git)", "i").test(a) ? a : "http://" + a;
                    case "urlstrict":
                        c = /^(https?|s?ftp|git):\/\/(((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:)*@)?(((\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5]))|((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?)(:\d*)?)(\/((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)+(\/(([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)*)*)?)?(\?((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)|[\uE000-\uF8FF]|\/|\?)*)?(#((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)|\/|\?)*)?$/i;
                        break;
                    case "dateIso":
                        c = /^(\d{4})\D?(0[1-9]|1[0-2])\D?([12]\d|0[1-9]|3[01])$/;
                        break;
                    case "phone":
                        c = /^((\+\d{1,3}(-| )?\(?\d\)?(-| )?\d{1,5})|(\(?\d{2,6}\)?))(-| )?(\d{3,4})(-| )?(\d{4})(( x| ext)\d{1,5}){0,1}$/;
                        break;
                    default:
                        return false
                }
                return "" !== a ? c.test(a) : false
            }, regexp: function (a, b, c) {
                return new RegExp(b, c.options.regexpFlag || "").test(a)
            }, minlength: function (a, b) {
                return a.length >= b
            }, maxlength: function (a, b) {
                return a.length <= b
            }, rangelength: function (a, b) {
                return this.minlength(a, b[0]) && this.maxlength(a, b[1])
            }, min: function (a, b) {
                return Number(a) >= b
            }, max: function (a, b) {
                return Number(a) <= b
            }, range: function (a, b) {
                return a >= b[0] && a <= b[1]
            }, equalto: function (b, c, d) {
                d.options.validateIfUnchanged = true;
                return b === a(c).val()
            }, remote: function (b, c, d) {
                var e = null, f = {}, g = {};
                f[d.$element.attr("name")] = b;
                if ("undefined" !== typeof d.options.remoteDatatype) {
                    g = { dataType: d.options.remoteDatatype }
                }
                var h = function (b, c) {
                    if ("undefined" !== typeof c && "undefined" !== typeof d.Validator.messages.remote && c !== d.Validator.messages.remote) {
                        a(d.ulError + " .remote").remove()
                    }
                    d.updtConstraint({ name: "remote", valid: b }, c);
                    d.manageValidationResult()
                };
                var i = function (b) {
                    if ("object" === typeof b) {
                        return b
                    }
                    try {
                        b = a.parseJSON(b)
                    } catch (c) {
                    }
                    return b
                };
                var j = function (a) {
                    return "object" === typeof a && null !== a ? "undefined" !== typeof a.error ? a.error : "undefined" !== typeof a.message ? a.message : null : null
                };
                a.ajax(a.extend({}, {
                    url: c, data: f, type: d.options.remoteMethod || "GET", success: function (a) {
                        a = i(a);
                        h(1 === a || true === a || "object" === typeof a && null !== a && "undefined" !== typeof a.success, j(a))
                    }, error: function (a) {
                        a = i(a);
                        h(false, j(a))
                    }
                }, g));
                return e
            }, mincheck: function (a, b) {
                return this.minlength(a, b)
            }, maxcheck: function (a, b) {
                return this.maxlength(a, b)
            }, rangecheck: function (a, b) {
                return this.rangelength(a, b)
            }
        }, init: function (a) {
            var b = a.validators, c = a.messages;
            var d;
            for (d in b) {
                this.addValidator(d, b[d])
            }
            for (d in c) {
                this.addMessage(d, c[d])
            }
        }, formatMesssage: function (a, b) {
            if ("object" === typeof b) {
                for (var c in b) {
                    a = this.formatMesssage(a, b[c])
                }
                return a
            }
            return "string" === typeof a ? a.replace(new RegExp("%s", "i"), b) : ""
        }, addValidator: function (a, b) {
            this.validators[a] = b
        }, addMessage: function (a, b, c) {
            if ("undefined" !== typeof c && true === c) {
                this.messages.type[a] = b;
                return
            }
            if ("type" === a) {
                for (var d in b) {
                    this.messages.type[d] = b[d]
                }
                return
            }
            this.messages[a] = b
        }
    };
    var c = function (a, c, d) {
        this.options = c;
        this.Validator = new b(c);
        if (d === "ParsleyFieldMultiple") {
            return this
        }
        this.init(a, d || "ParsleyField")
    };
    c.prototype = {
        constructor: c, init: function (b, c) {
            this.type = c;
            this.valid = true;
            this.element = b;
            this.validatedOnce = false;
            this.$element = a(b);
            this.val = this.$element.val();
            this.isRequired = false;
            this.constraints = {};
            if ("undefined" === typeof this.isRadioOrCheckbox) {
                this.isRadioOrCheckbox = false;
                this.hash = this.generateHash();
                this.errorClassHandler = this.options.errors.classHandler(b, this.isRadioOrCheckbox) || this.$element
            }
            this.ulErrorManagement();
            this.bindHtml5Constraints();
            this.addConstraints();
            if (this.hasConstraints()) {
                this.bindValidationEvents()
            }
        }, setParent: function (b) {
            this.$parent = a(b)
        }, getParent: function () {
            return this.$parent
        }, bindHtml5Constraints: function () {
            if (this.$element.hasClass("required") || this.$element.prop("required")) {
                this.options.required = true
            }
            if ("undefined" !== typeof this.$element.attr("type") && new RegExp(this.$element.attr("type"), "i").test("email url number range")) {
                this.options.type = this.$element.attr("type");
                if (new RegExp(this.options.type, "i").test("number range")) {
                    this.options.type = "number";
                    if ("undefined" !== typeof this.$element.attr("min") && this.$element.attr("min").length) {
                        this.options.min = this.$element.attr("min")
                    }
                    if ("undefined" !== typeof this.$element.attr("max") && this.$element.attr("max").length) {
                        this.options.max = this.$element.attr("max")
                    }
                }
            }
            if ("string" === typeof this.$element.attr("pattern") && this.$element.attr("pattern").length) {
                this.options.regexp = this.$element.attr("pattern")
            }
        }, addConstraints: function () {
            for (var a in this.options) {
                var b = {};
                b[a] = this.options[a];
                this.addConstraint(b, true)
            }
        }, addConstraint: function (a, b) {
            for (var c in a) {
                c = c.toLowerCase();
                if ("function" === typeof this.Validator.validators[c]) {
                    this.constraints[c] = { name: c, requirements: a[c], valid: null };
                    if (c === "required") {
                        this.isRequired = true
                    }
                    this.addCustomConstraintMessage(c)
                }
            }
            if ("undefined" === typeof b) {
                this.bindValidationEvents()
            }
        }, updateConstraint: function (a, b) {
            for (var c in a) {
                this.updtConstraint({ name: c, requirements: a[c], valid: null }, b)
            }
        }, updtConstraint: function (b, c) {
            this.constraints[b.name] = a.extend(true, this.constraints[b.name], b);
            if ("string" === typeof c) {
                this.Validator.messages[b.name] = c
            }
            this.bindValidationEvents()
        }, removeConstraint: function (a) {
            var a = a.toLowerCase();
            delete this.constraints[a];
            if (a === "required") {
                this.isRequired = false
            }
            if (!this.hasConstraints()) {
                if ("ParsleyForm" === typeof this.getParent()) {
                    this.getParent().removeItem(this.$element);
                    return
                }
                this.destroy();
                return
            }
            this.bindValidationEvents()
        }, addCustomConstraintMessage: function (a) {
            var b = a + ("type" === a && "undefined" !== typeof this.options[a] ? this.options[a].charAt(0).toUpperCase() + this.options[a].substr(1) : "") + "Message";
            if ("undefined" !== typeof this.options[b]) {
                this.Validator.addMessage("type" === a ? this.options[a] : a, this.options[b], "type" === a)
            }
        }, bindValidationEvents: function () {
            this.valid = null;
            this.$element.addClass("parsley-validated");
            this.$element.off("." + this.type);
            if (this.options.remote && !new RegExp("change", "i").test(this.options.trigger)) {
                this.options.trigger = !this.options.trigger ? "change" : " change"
            }
            var b = (!this.options.trigger ? "" : this.options.trigger) + (new RegExp("key", "i").test(this.options.trigger) ? "" : " keyup");
            if (this.$element.is("select")) {
                b += new RegExp("change", "i").test(b) ? "" : " change"
            }
            b = b.replace(/^\s+/g, "").replace(/\s+$/g, "");
            this.$element.on((b + " ").split(" ").join("." + this.type + " "), false, a.proxy(this.eventValidation, this))
        }, generateHash: function () {
            return "parsley-" + (Math.random() + "").substring(2)
        }, getHash: function () {
            return this.hash
        }, getVal: function () {
            return this.$element.data("value") || this.$element.val()
        }, eventValidation: function (a) {
            var b = this.getVal();
            if (a.type === "keyup" && !/keyup/i.test(this.options.trigger) && !this.validatedOnce) {
                return true
            }
            if (a.type === "change" && !/change/i.test(this.options.trigger) && !this.validatedOnce) {
                return true
            }
            if (!this.isRadioOrCheckbox && b.length < this.options.validationMinlength && !this.validatedOnce) {
                return true
            }
            this.validate()
        }, isValid: function () {
            return this.validate(false)
        }, hasConstraints: function () {
            for (var a in this.constraints) {
                return true
            }
            return false
        }, validate: function (a) {
            var b = this.getVal(), c = null;
            if (!this.hasConstraints()) {
                return null
            }
            if (this.options.listeners.onFieldValidate(this.element, this) || "" === b && !this.isRequired) {
                this.reset();
                return null
            }
            if (!this.needsValidation(b)) {
                return this.valid
            }
            c = this.applyValidators();
            if ("undefined" !== typeof a ? a : this.options.showErrors) {
                this.manageValidationResult()
            }
            return c
        }, needsValidation: function (a) {
            if (!this.options.validateIfUnchanged && this.valid !== null && this.val === a && this.validatedOnce) {
                return false
            }
            this.val = a;
            return this.validatedOnce = true
        }, applyValidators: function () {
            var a = null;
            for (var b in this.constraints) {
                var c = this.Validator.validators[this.constraints[b].name](this.val, this.constraints[b].requirements, this);
                if (false === c) {
                    a = false;
                    this.constraints[b].valid = a;
                    this.options.listeners.onFieldError(this.element, this.constraints, this)
                } else if (true === c) {
                    this.constraints[b].valid = true;
                    a = false !== a;
                    this.options.listeners.onFieldSuccess(this.element, this.constraints, this)
                }
            }
            return a
        }, manageValidationResult: function () {
            var a = null;
            for (var b in this.constraints) {
                if (false === this.constraints[b].valid) {
                    this.manageError(this.constraints[b]);
                    a = false
                } else if (true === this.constraints[b].valid) {
                    this.removeError(this.constraints[b].name);
                    a = false !== a
                }
            }
            this.valid = a;
            if (true === this.valid) {
                this.removeErrors();
                this.errorClassHandler.removeClass(this.options.errorClass).addClass(this.options.successClass);
                return true
            } else if (false === this.valid) {
                this.errorClassHandler.removeClass(this.options.successClass).addClass(this.options.errorClass);
                return false
            }
            return a
        }, ulErrorManagement: function () {
            this.ulError = "#" + this.hash;
            this.ulTemplate = a(this.options.errors.errorsWrapper).attr("id", this.hash).addClass("parsley-error-list")
        }, removeError: function (b) {
            var c = this.ulError + " ." + b, d = this;
            this.options.animate ? a(c).fadeOut(this.options.animateDuration, function () {
                a(this).remove();
                if (d.ulError && a(d.ulError).children().length === 0) {
                    d.removeErrors()
                }
            }) : a(c).remove();
            if (this.ulError && a(this.ulError).children().length === 0) {
                this.removeErrors()
            }
        }, addError: function (b) {
            for (var c in b) {
                var d = a(this.options.errors.errorElem).addClass(c);
                a(this.ulError).append(this.options.animate ? a(d).html(b[c]).hide().fadeIn(this.options.animateDuration) : a(d).html(b[c]))
            }
        }, removeErrors: function () {
            this.options.animate ? a(this.ulError).fadeOut(this.options.animateDuration, function () {
                a(this).remove()
            }) : a(this.ulError).remove()
        }, reset: function () {
            this.valid = null;
            this.removeErrors();
            this.validatedOnce = false;
            this.errorClassHandler.removeClass(this.options.successClass).removeClass(this.options.errorClass);
            for (var a in this.constraints) {
                this.constraints[a].valid = null
            }
            return this
        }, manageError: function (b) {
            if (!a(this.ulError).length) {
                this.manageErrorContainer()
            }
            if ("required" === b.name && null !== this.getVal() && this.getVal().length > 0) {
                return
            } else if (this.isRequired && "required" !== b.name && (null === this.getVal() || 0 === this.getVal().length)) {
                return
            }
            var c = b.name, d = false !== this.options.errorMessage ? "custom-error-message" : c, e = {}, f = false !== this.options.errorMessage ? this.options.errorMessage : b.name === "type" ? this.Validator.messages[c][b.requirements] : "undefined" === typeof this.Validator.messages[c] ? this.Validator.messages.defaultMessage : this.Validator.formatMesssage(this.Validator.messages[c], b.requirements);
            if (!a(this.ulError + " ." + d).length) {
                e[d] = f;
                this.addError(e)
            }
        }, manageErrorContainer: function () {
            var b = this.options.errorContainer || this.options.errors.container(this.element, this.isRadioOrCheckbox), c = this.options.animate ? this.ulTemplate.show() : this.ulTemplate;
            if ("undefined" !== typeof b) {
                a(b).append(c);
                return
            }
            !this.isRadioOrCheckbox ? this.$element.after(c) : this.$element.parent().after(c)
        }, addListener: function (a) {
            for (var b in a) {
                this.options.listeners[b] = a[b]
            }
        }, destroy: function () {
            this.$element.removeClass("parsley-validated");
            this.reset().$element.off("." + this.type).removeData(this.type)
        }
    };
    var d = function (a, c, d) {
        this.initMultiple(a, c);
        this.inherit(a, c);
        this.Validator = new b(c);
        this.init(a, d || "ParsleyFieldMultiple")
    };
    d.prototype = {
        constructor: d, initMultiple: function (b, c) {
            this.element = b;
            this.$element = a(b);
            this.group = c.group || false;
            this.hash = this.getName();
            this.siblings = this.group ? '[data-group="' + this.group + '"]' : 'input[name="' + this.$element.attr("name") + '"]';
            this.isRadioOrCheckbox = true;
            this.isRadio = this.$element.is("input[type=radio]");
            this.isCheckbox = this.$element.is("input[type=checkbox]");
            this.errorClassHandler = c.errors.classHandler(b, this.isRadioOrCheckbox) || this.$element.parent()
        }, inherit: function (a, b) {
            var d = new c(a, b, "ParsleyFieldMultiple");
            for (var e in d) {
                if ("undefined" === typeof this[e]) {
                    this[e] = d[e]
                }
            }
        }, getName: function () {
            if (this.group) {
                return "parsley-" + this.group
            }
            if ("undefined" === typeof this.$element.attr("name")) {
                throw "A radio / checkbox input must have a data-group attribute or a name to be Parsley validated !"
            }
            return "parsley-" + this.$element.attr("name").replace(/(:|\.|\[|\])/g, "")
        }, getVal: function () {
            if (this.isRadio) {
                return a(this.siblings + ":checked").val() || ""
            }
            if (this.isCheckbox) {
                var b = [];
                a(this.siblings + ":checked").each(function () {
                    b.push(a(this).val())
                });
                return b
            }
        }, bindValidationEvents: function () {
            this.valid = null;
            this.$element.addClass("parsley-validated");
            this.$element.off("." + this.type);
            var b = this, c = (!this.options.trigger ? "" : this.options.trigger) + (new RegExp("change", "i").test(this.options.trigger) ? "" : " change");
            c = c.replace(/^\s+/g, "").replace(/\s+$/g, "");
            a(this.siblings).each(function () {
                a(this).on(c.split(" ").join("." + b.type + " "), false, a.proxy(b.eventValidation, b))
            })
        }
    };
    var e = function (a, b, c) {
        this.init(a, b, c || "parsleyForm")
    };
    e.prototype = {
        constructor: e, init: function (b, c, d) {
            this.type = d;
            this.items = [];
            this.$element = a(b);
            this.options = c;
            var e = this;
            this.$element.find(c.inputs).each(function () {
                e.addItem(this)
            });
            this.$element.on("submit." + this.type, false, a.proxy(this.validate, this))
        }, addListener: function (a) {
            for (var b in a) {
                if (new RegExp("Field").test(b)) {
                    for (var c = 0; c < this.items.length; c++) {
                        this.items[c].addListener(a)
                    }
                } else {
                    this.options.listeners[b] = a[b]
                }
            }
        }, addItem: function (b) {
            if (a(b).is(this.options.excluded)) {
                return false
            }
            var c = a(b).parsley(this.options);
            c.setParent(this);
            this.items.push(c)
        }, removeItem: function (b) {
            var c = a(b).parsley();
            for (var d = 0; d < this.items.length; d++) {
                if (this.items[d].hash === c.hash) {
                    this.items[d].destroy();
                    this.items.splice(d, 1);
                    return true
                }
            }
            return false
        }, validate: function (a) {
            var b = true;
            this.focusedField = false;
            for (var c = 0; c < this.items.length; c++) {
                if ("undefined" !== typeof this.items[c] && false === this.items[c].validate()) {
                    b = false;
                    if (!this.focusedField && "first" === this.options.focus || "last" === this.options.focus) {
                        this.focusedField = this.items[c].$element
                    }
                }
            }
            if (this.focusedField && !b) {
                this.focusedField.focus()
            }
            this.options.listeners.onFormSubmit(b, a, this);
            return b
        }, isValid: function () {
            for (var a = 0; a < this.items.length; a++) {
                if (false === this.items[a].isValid()) {
                    return false
                }
            }
            return true
        }, removeErrors: function () {
            for (var a = 0; a < this.items.length; a++) {
                this.items[a].parsley("reset")
            }
        }, destroy: function () {
            for (var a = 0; a < this.items.length; a++) {
                this.items[a].destroy()
            }
            this.$element.off("." + this.type).removeData(this.type)
        }, reset: function () {
            for (var a = 0; a < this.items.length; a++) {
                this.items[a].reset()
            }
        }
    };
    a.fn.parsley = function (b, f) {
        var g = a.extend(true, {}, a.fn.parsley.defaults, "undefined" !== typeof window.ParsleyConfig ? window.ParsleyConfig : {}, b, this.data()), h = null;

        function i(h, i) {
            var j = a(h).data(i);
            if (!j) {
                switch (i) {
                    case "parsleyForm":
                        j = new e(h, g, "parsleyForm");
                        break;
                    case "parsleyField":
                        j = new c(h, g, "parsleyField");
                        break;
                    case "parsleyFieldMultiple":
                        j = new d(h, g, "parsleyFieldMultiple");
                        break;
                    default:
                        return
                }
                a(h).data(i, j)
            }
            if ("string" === typeof b && "function" === typeof j[b]) {
                var k = j[b](f);
                return "undefined" !== typeof k ? k : a(h)
            }
            return j
        }

        if (a(this).is("form") || true === a(this).data("bind")) {
            h = i(a(this), "parsleyForm")
        } else if (a(this).is(g.inputs) && !a(this).is(g.excluded)) {
            h = i(a(this), !a(this).is("input[type=radio], input[type=checkbox]") ? "parsleyField" : "parsleyFieldMultiple")
        }
        return "function" === typeof f ? f() : h
    };
    a.fn.parsley.Constructor = e;
    a.fn.parsley.defaults = {
        inputs: "input, textarea, select", excluded: "input[type=hidden], input[type=file], :disabled", trigger: false, animate: true, animateDuration: 300, focus: "first", validationMinlength: 3, successClass: "parsley-success", errorClass: "parsley-error", errorMessage: false, validators: {}, showErrors: true, messages: {}, validateIfUnchanged: false, errors: {
            classHandler: function (a, b) {
            }, container: function (a, b) {
            }, errorsWrapper: "<ul></ul>", errorElem: "<li></li>"
        }, listeners: {
            onFieldValidate: function (a, b) {
                return false
            }, onFormSubmit: function (a, b, c) {
            }, onFieldError: function (a, b, c) {
            }, onFieldSuccess: function (a, b, c) {
            }
        }
    };
    a(window).on("load", function () {
        a('[data-validate="parsley"]').each(function () {
            a(this).parsley()
        })
    })
}(window.jQuery || window.Zepto);
var BrowserDetect = {
    init: function () {
        this.browser = this.searchString(this.dataBrowser) || "An unknown browser";
        this.version = this.searchVersion(navigator.userAgent) || this.searchVersion(navigator.appVersion) || "an unknown version";
        this.OS = this.searchString(this.dataOS) || "an unknown OS"
    }, searchString: function (a) {
        for (var b = 0; b < a.length; b++) {
            var c = a[b].string;
            var d = a[b].prop;
            this.versionSearchString = a[b].versionSearch || a[b].identity;
            if (c) {
                if (c.indexOf(a[b].subString) != -1) return a[b].identity
            } else if (d) return a[b].identity
        }
    }, searchVersion: function (a) {
        var b = a.indexOf(this.versionSearchString);
        if (b == -1) return;
        return parseFloat(a.substring(b + this.versionSearchString.length + 1))
    }, dataBrowser: [
        { string: navigator.userAgent, subString: "Chrome", identity: "Chrome" },
        { string: navigator.userAgent, subString: "OmniWeb", versionSearch: "OmniWeb/", identity: "OmniWeb" },
        { string: navigator.vendor, subString: "Apple", identity: "Safari", versionSearch: "Version" },
        { prop: window.opera, identity: "Opera", versionSearch: "Version" },
        { string: navigator.vendor, subString: "iCab", identity: "iCab" },
        { string: navigator.vendor, subString: "KDE", identity: "Konqueror" },
        { string: navigator.userAgent, subString: "Firefox", identity: "Firefox" },
        { string: navigator.vendor, subString: "Camino", identity: "Camino" },
        { string: navigator.userAgent, subString: "Netscape", identity: "Netscape" },
        { string: navigator.userAgent, subString: "MSIE", identity: "Explorer", versionSearch: "MSIE" },
        { string: navigator.userAgent, subString: "Gecko", identity: "Mozilla", versionSearch: "rv" },
        { string: navigator.userAgent, subString: "Mozilla", identity: "Netscape", versionSearch: "Mozilla" }
    ], dataOS: [
        { string: navigator.platform, subString: "Win", identity: "Windows" },
        { string: navigator.platform, subString: "Mac", identity: "Mac" },
        { string: navigator.userAgent, subString: "iPhone", identity: "iPhone/iPod" },
        { string: navigator.platform, subString: "Linux", identity: "Linux" }
    ]
};
BrowserDetect.init();
(function (a, b) {
    if (typeof exports == "object") module.exports = b(); else if (typeof define == "function" && define.amd) define(b); else a.Spinner = b()
})(this, function () {
    "use strict";
    var a = ["webkit", "Moz", "ms", "O"], b = {}, c;

    function d(a, b) {
        var c = document.createElement(a || "div"), d;
        for (d in b) c[d] = b[d];
        return c
    }

    function e(a) {
        for (var b = 1, c = arguments.length; b < c; b++) a.appendChild(arguments[b]);
        return a
    }

    var f = function () {
        var a = d("style", { type: "text/css" });
        e(document.getElementsByTagName("head")[0], a);
        return a.sheet || a.styleSheet
    }();

    function g(a, d, e, g) {
        var h = ["opacity", d, ~~(a * 100), e, g].join("-"), i = .01 + e / g * 100, j = Math.max(1 - (1 - a) / d * (100 - i), a), k = c.substring(0, c.indexOf("Animation")).toLowerCase(), l = k && "-" + k + "-" || "";
        if (!b[h]) {
            f.insertRule("@" + l + "keyframes " + h + "{" + "0%{opacity:" + j + "}" + i + "%{opacity:" + a + "}" + (i + .01) + "%{opacity:1}" + (i + d) % 100 + "%{opacity:" + a + "}" + "100%{opacity:" + j + "}" + "}", f.cssRules.length);
            b[h] = 1
        }
        return h
    }

    function h(b, c) {
        var d = b.style, e, f;
        if (d[c] !== undefined) return c;
        c = c.charAt(0).toUpperCase() + c.slice(1);
        for (f = 0; f < a.length; f++) {
            e = a[f] + c;
            if (d[e] !== undefined) return e
        }
    }

    function i(a, b) {
        for (var c in b) a.style[h(a, c) || c] = b[c];
        return a
    }

    function j(a) {
        for (var b = 1; b < arguments.length; b++) {
            var c = arguments[b];
            for (var d in c) if (a[d] === undefined) a[d] = c[d]
        }
        return a
    }

    function k(a) {
        var b = { x: a.offsetLeft, y: a.offsetTop };
        while (a = a.offsetParent) b.x += a.offsetLeft, b.y += a.offsetTop;
        return b
    }

    var l = { lines: 12, length: 7, width: 5, radius: 10, rotate: 0, corners: 1, color: "#000", direction: 1, speed: 1, trail: 100, opacity: 1 / 4, fps: 20, zIndex: 2e9, className: "spinner", top: "auto", left: "auto", position: "relative" };

    function m(a) {
        if (typeof this == "undefined") return new m(a);
        this.opts = j(a || {}, m.defaults, l)
    }

    m.defaults = {};
    j(m.prototype, {
        spin: function (a) {
            this.stop();
            var b = this, e = b.opts, f = b.el = i(d(0, { className: e.className }), { position: e.position, width: 0, zIndex: e.zIndex }), g = e.radius + e.length + e.width, h, j;
            if (a) {
                a.insertBefore(f, a.firstChild || null);
                j = k(a);
                h = k(f);
                i(f, { left: (e.left == "auto" ? j.x - h.x + (a.offsetWidth >> 1) : parseInt(e.left, 10) + g) + "px", top: (e.top == "auto" ? j.y - h.y + (a.offsetHeight >> 1) : parseInt(e.top, 10) + g) + "px" })
            }
            f.setAttribute("role", "progressbar");
            b.lines(f, b.opts);
            if (!c) {
                var l = 0, m = (e.lines - 1) * (1 - e.direction) / 2, n, o = e.fps, p = o / e.speed, q = (1 - e.opacity) / (p * e.trail / 100), r = p / e.lines;
                (function s() {
                    l++;
                    for (var a = 0; a < e.lines; a++) {
                        n = Math.max(1 - (l + (e.lines - a) * r) % p * q, e.opacity);
                        b.opacity(f, a * e.direction + m, n, e)
                    }
                    b.timeout = b.el && setTimeout(s, ~~(1e3 / o))
                })()
            }
            return b
        }, stop: function () {
            var a = this.el;
            if (a) {
                clearTimeout(this.timeout);
                if (a.parentNode) a.parentNode.removeChild(a);
                this.el = undefined
            }
            return this
        }, lines: function (a, b) {
            var f = 0, h = (b.lines - 1) * (1 - b.direction) / 2, j;

            function k(a, c) {
                return i(d(), { position: "absolute", width: b.length + b.width + "px", height: b.width + "px", background: a, boxShadow: c, transformOrigin: "left", transform: "rotate(" + ~~(360 / b.lines * f + b.rotate) + "deg) translate(" + b.radius + "px" + ",0)", borderRadius: (b.corners * b.width >> 1) + "px" })
            }

            for (; f < b.lines; f++) {
                j = i(d(), { position: "absolute", top: 1 + ~(b.width / 2) + "px", transform: b.hwaccel ? "translate3d(0,0,0)" : "", opacity: b.opacity, animation: c && g(b.opacity, b.trail, h + f * b.direction, b.lines) + " " + 1 / b.speed + "s linear infinite" });
                if (b.shadow) e(j, i(k("#000", "0 0 4px " + "#000"), { top: 2 + "px" }));
                e(a, e(j, k(b.color, "0 0 1px rgba(0,0,0,.1)")))
            }
            return a
        }, opacity: function (a, b, c) {
            if (b < a.childNodes.length) a.childNodes[b].style.opacity = c
        }
    });
    function n() {
        function a(a, b) {
            return d("<" + a + ' xmlns="urn:schemas-microsoft.com:vml" class="spin-vml">', b)
        }

        f.addRule(".spin-vml", "behavior:url(#default#VML)");
        m.prototype.lines = function (b, c) {
            var d = c.length + c.width, f = 2 * d;

            function g() {
                return i(a("group", { coordsize: f + " " + f, coordorigin: -d + " " + -d }), { width: f, height: f })
            }

            var h = -(c.width + c.length) * 2 + "px", j = i(g(), { position: "absolute", top: h, left: h }), k;

            function l(b, f, h) {
                e(j, e(i(g(), { rotation: 360 / c.lines * b + "deg", left: ~~f }), e(i(a("roundrect", { arcsize: c.corners }), { width: d, height: c.width, left: c.radius, top: -c.width >> 1, filter: h }), a("fill", { color: c.color, opacity: c.opacity }), a("stroke", { opacity: 0 }))))
            }

            if (c.shadow) for (k = 1; k <= c.lines; k++) l(k, -2, "progid:DXImageTransform.Microsoft.Blur(pixelradius=2,makeshadow=1,shadowopacity=.3)");
            for (k = 1; k <= c.lines; k++) l(k);
            return e(b, j)
        };
        m.prototype.opacity = function (a, b, c, d) {
            var e = a.firstChild;
            d = d.shadow && d.lines || 0;
            if (e && b + d < e.childNodes.length) {
                e = e.childNodes[b + d];
                e = e && e.firstChild;
                e = e && e.firstChild;
                if (e) e.opacity = c
            }
        }
    }

    var o = i(d("group"), { behavior: "url(#default#VML)" });
    if (!h(o, "transform") && o.adj) n(); else c = h(o, "animation");
    return m
});
var RemoteValidator = function (a) {
    var b = {};
    b.spinner = "";
    b.clearRemoteValidations = function (b) {
        a(b).find("ul.parsley-error-list-server").remove()
    };
    b.isLoading = function (b) {
        if (a(b).is("form")) {
            return a(b).hasClass("form-loading")
        } else {
            return a(b).closest("form").hasClass("form-loading")
        }
    };
    b.toggleLoading = function (a) {
        if (this.isLoading(a)) {
            a.find("button.btn-submit").removeClass("btn-loading").removeClass("disabled")
        } else {
            a.find("button.btn-submit").addClass("btn-loading disabled")
        }
    };
    b.toggleSpinner = function (c, d) {
        var e = document.getElementById(c);
        $target = a(e);
        var d = a.extend({ lines: 13, length: 20, width: 10, radius: 30, corners: 1, rotate: 0, direction: 1, color: "#441c5b", speed: 1, trail: 60, shadow: false, hwaccel: false, className: "hri-spinner", zIndex: 2e9, top: "auto", left: "auto" }, d);
        if ($target.find(".hri-spinner").length) {
            $target.find("#hri-spinner-overlay").remove();
            b.spinner.stop()
        } else {
            var f = '<div id="hri-spinner-overlay"></div>';
            $target.append(f);
            b.spinner = new Spinner(d).spin(e)
        }
    };
    b.getCookie = function (a) {
        var b = null;
        if (document.cookie && document.cookie !== "") {
            var c = document.cookie.split(";");
            for (var d = 0; d < c.length; d++) {
                var e = jQuery.trim(c[d]);
                if (e.substring(0, a.length + 1) == a + "=") {
                    b = decodeURIComponent(e.substring(a.length + 1));
                    break
                }
            }
        }
        return b
    };
    b.handleResponse = function (b) {
        if ("object" === typeof b) {
            return b
        }
        try {
            b = a.parseJSON(b)
        } catch (c) {
        }
        return b
    };
    b.handleErrors = function (b, c) {
        var d = false;
        if (c.hasClass("shop-form")) {
            d = true
        }
        if (b.hasOwnProperty("errors")) {
            for (var e = 0; e < b.errors.length; e++) {
                var f = b.errors[e], g = "", h = "";
                for (var i in f) {
                    g = a('[name="' + i + '"]', c);
                    h = f[i];
                    g.after('<ul class="parsley-error-list-server"></ul>');
                    var j = g.next("ul.parsley-error-list-server");
                    j.append("<li>" + h + "</li>");
                    if (d) {
                        g.closest(".shop-block").find(".shop-btn.shop-edit").click()
                    }
                }
            }
        }
    };
    b.handleSuccess = function (b) {
        $form.find('input[type!="hidden"]').each(function () {
            a(this).val("")
        })
    };
    b.validate = function (c, d, e) {
        var e = e || false;
        b.clearRemoteValidations(c);
        if (d) {
            b.toggleSpinner(d)
        }
        var f = null, g = {}, h = c.attr("action") || c.attr("remotevalidate-url"), i = c.attr("method") || "POST";
        g = c.serialize();
        a.ajax(a.extend({}, {
            url: h, data: g, type: i, dataType: "json", contentType: "application/json", beforeSend: function (a, c) {
                if ("POST" === i.toUpperCase()) {
                    if (!(/^http:.*/.test(c.url) || /^https:.*/.test(c.url))) {
                        a.setRequestHeader("X-CSRFToken", b.getCookie("csrftoken"))
                    }
                }
            }, success: function (a) {
                if (a.status === "success") {
                    var f = a.payloads[0];
                    if (f.redirect_to) {
                        window.location = f.redirect_to
                    }
                    if (d) {
                        if (!e) {
                            b.toggleSpinner(d)
                        }
                    }
                } else if (a.status === "failure") {
                    b.handleErrors(a, c);
                    if (d) {
                        b.toggleSpinner(d)
                    }
                } else {
                    $shopBlock.data("stepvalid", false)
                }
            }, error: function (a) {
                a = b.handleResponse(a.responseText);
                b.handleErrors(a)
            }
        })).done(function () {
        })
    };
    return b
}(jQuery);
(function () {
    $(document).ready(function () {
        var a = function (a) {
            a.preventDefault();
            var b = $(this), c = b.closest(".hri-accordion"), d = b.find(".hri-accordion-icon"), e = b.closest(".hri-accordion").find(".hri-accordion-content");
            e.stop().slideToggle(function () {
                c.toggleClass("hri-accordion-state-expanded");
                return true
            })
        };
        var b = function (a) {
            a.preventDefault();
            var b = $(this), c = b.hasClass("hri-accordion-toggle-all-state-expanded"), d = b.text(), e = b.data("accordion-toggle-all-text");
            if (c) {
                b.text(e);
                b.data("accordion-toggle-all-text", d);
                b.removeClass("hri-accordion-toggle-all-state-expanded");
                $(".hri-accordion").each(function () {
                    var a = $(this);
                    if (a.hasClass("hri-accordion-state-expanded")) {
                        a.find(".hri-accordion-header").click()
                    } else {
                    }
                })
            } else {
                b.text(e);
                b.data("accordion-toggle-all-text", d);
                b.addClass("hri-accordion-toggle-all-state-expanded");
                $(".hri-accordion").each(function () {
                    var a = $(this);
                    if (a.hasClass("hri-accordion-state-expanded")) {
                    } else {
                        a.find(".hri-accordion-header").click()
                    }
                })
            }
        };
        $(".hri-accordion").on("click.toggle-one.hri-accordion", "div.hri-accordion-header", a);
        $(".hri-accordion-toggle-all").on("click.toggle-all.hri-accordion", null, b)
    })
})();
(function () {
    $(document).ready(function () {
        $("#hri-carousel").carousel();
        $("#tcMore").carousel({ interval: 0 });
        $(".hri-tooltip").tooltip({ placement: "bottom" });
        $(".hri-tooltip").click(function (a) {
            a.preventDefault()
        });
        $("#hri-more ol.carousel-indicators a").click(function (a) {
            a.preventDefault();
            var b = $(this);
            var c = b.attr("href").slice(1);
            b.closest("#hri-more").removeAttr("class").addClass(c)
        });
        $("#hri-subscribe-modal.modal").on("click", "button.btn-submit", function (a) {
            var c = $(this).closest("form[data-validate=parsley]");
            if (b.isLoading(c)) {
                a.preventDefault()
            } else {
                c.find("div.form-success").remove();
                var d = c.parsley("isValid");
                if (d) {
                    a.preventDefault();
                    b.validate(c)
                } else {
                }
            }
        });
        $(".parsley-form").parsley("addListener", {
            onFieldValidate: function (a) {
                if ($(a).is(":visible")) {
                    var b = $(a).closest("div").find(".errorlist");
                    if (b.length !== 0) {
                        b.remove()
                    }
                }
            }
        });
        var a = window.location.href;
        if (a.toLowerCase().indexOf("sso/portal/billing") === -1) {
            $("#js-hri-enrollment-nav").text("Resume Enrollment");
            $("#js-hri-linkexacc-nav").show()
        } else {
            $("#js-hri-enrollment-nav").text("Exit Enrollment");
            $("#js-hri-linkexacc-nav").hide()
        }
        var b = {
            clearRemoteValidations: function (a) {
                a.find("ul.parsley-error-list-server").remove()
            }, isLoading: function (a) {
                return a.find("button.btn-submit").hasClass("btn-loading")
            }, toggleLoading: function (a) {
                if (this.isLoading(a)) {
                    a.find("button.btn-submit").removeClass("btn-loading").removeClass("disabled")
                } else {
                    a.find("button.btn-submit").addClass("btn-loading disabled")
                }
            }, validate: function (a) {
                this.clearRemoteValidations(a);
                var c = {}, d = "";
                this.toggleLoading(a);
                c = a.serializeArray();
                d = a.data("remotevalidate-url");
                var e = function (a) {
                    if ("object" === typeof a) {
                        return a
                    }
                    try {
                        a = $.parseJSON(a)
                    } catch (b) {
                    }
                    return a
                };
                var f = function (b) {
                    for (var c in b) {
                        if (b.hasOwnProperty(c)) {
                            var d = b[c].length;
                            var e = $('[name="' + c + '"]', a);
                            e.after('<ul class="parsley-error-list-server"></ul>');
                            var f = e.next("ul.parsley-error-list-server");
                            for (var g = 0; g < d; g++) {
                                f.append("<li>" + b[c][g] + "</li>")
                            }
                        }
                    }
                };
                var g = function () {
                    a.prepend('<div class="alert form-success">Thank you for signing up!</div>');
                    a.find('input[type!="hidden"]').each(function () {
                        $(this).val("")
                    });
                    if (hri.siteName === "ny") {
                        document.body.insertAdjacentHTML("beforeend", '<img id="img111" src="https://adfarm.mediaplex.com/ad/bk/25828-186691-3840-0?LeadGen_NY=1&mpuid=" height="1" width="1" alt="Mediaplex_tag" />')
                    } else if (hri.siteName === "nj") {
                        document.body.insertAdjacentHTML("beforeend", '<img id="img222" src="https://adfarm.mediaplex.com/ad/bk/25828-186691-3840-0?LeadGen_NJ=1&mpuid=" height="1" width="1" alt="Mediaplex_tag" />')
                    } else {
                    }
                };
                $.ajax($.extend({}, {
                    url: d, data: c, type: "POST", success: function (a) {
                        g(a)
                    }, error: function (a) {
                        a = e(a.responseText);
                        f(a)
                    }
                }, "jsonp")).done(function () {
                    b.toggleLoading(a)
                })
            }
        };
        $('input[type="submit"]').dblclick(function () {
            return false
        })
    })
})();
var HRI;
(function () {
    var a;
    HRI = function b() {
        if (a) {
            return a
        }
        a = this;
        var b = $("body").attr("class");
        var c = "";
        if (b.indexOf("newyork") !== -1) {
            c = "ny"
        } else if (b.indexOf("newjersey") !== -1) {
            c = "nj"
        } else if (b.indexOf("oregon") !== -1) {
            c = "or"
        } else if (b.indexOf("national") !== -1) {
            c = "nat"
        } else {
            c = "cmn"
        }
        "0";
        this.getSiteName = function () {
            return c
        };
        this.siteName = this.getSiteName()
    }
})();
var hri = new HRI;
var preventCopyPaste = function (a) {
    $.each(a, function (a, b) {
        try {
            $(b).bind("copy paste", function (a) {
                a.preventDefault()
            })
        } catch (c) {
            alert(c)
        }
    })
};
(function () {
    $(document).ready(function () {
        var a = true;
        var b = {};
        b.tenant = "national";
        var c = function () {
            if ($("body").hasClass("hri-newyork")) {
                b.tenant = "ny"
            } else if ($("body").hasClass("hri-newjersey")) {
                b.tenant = "nj"
            } else if ($("body").hasClass("hri-oregon")) {
                b.tenant = "or"
            }
        };
        c();
        $('.shop-block input[type="checkbox"]').each(function () {
            if ($(this).prop("checked")) {
                $(this).closest(".field-block").find(".hri-hide").removeClass("hri-hide")
            }
        });
        $(".shop-form").parsley("addListener", {
            onFieldValidate: function (a) {
                if ($(a).is(":visible")) {
                    var b = $(a).closest("div").find(".parsley-error-list-server");
                    if (b.length !== 0) {
                        b.remove()
                    }
                }
            }
        });
        
        var f = function (a) {
            var b = a.split("-");
            return b
        };
        var g = function (a) {
            var b = f(hri_planEffectiveDate);
            var c = new Date(parseInt(b[0], 10), parseInt(b[1], 10) - 1, parseInt(b[2], 10));
            b = f(a);
            var a = new Date(parseInt(b[0], 10), parseInt(b[1], 10) - 1, parseInt(b[2], 10)), d = c.getFullYear() - a.getFullYear(), e = c.getMonth() - a.getMonth();
            if (e < 0 || e === 0 && c.getDate() < a.getDate()) {
                d = d - 1
            }
            return d
        };
        var h = function () {
            var b = true;
            $(".shop-block").each(function () {
                b = b && $(this).data("stepvalid")
            });
            if (b) {
                $("#shop-form .shop-submit").removeClass("disabled");
                a = true
            } else {
                $("#shop-form .shop-submit").addClass("disabled");
                a = false
            }
        };
        var i = function (a, b) {
            var c = a.find(".textify-block");
            $fieldBlock = a.find(".field-block");
            if (c.hasClass("hri-hide")) {
                c.find("div").html(b);
                c.removeClass("hri-hide");
                $fieldBlock.addClass("hri-hide")
            } else {
                c.addClass("hri-hide");
                $fieldBlock.removeClass("hri-hide")
            }
        };
    })
})();
(function (a) {
    a(function () {
        a(".scroller").click(function () {
            var b = a(this).prop("hash");
            var c = a(b);
            if (b && c.length) {
                a("html, body").animate({ scrollTop: c.offset().top }, "slow", function () {
                    document.location.hash = b
                });
                return false
            }
        });
        a(".expand-content").hide();
        a(".expand-all").click(function () {
            var b = a(this);
            var c = a(".expand-group");
            var d = c.children(".expand-content");
            var e = a(".expand-toggle");
            if (d.is(":hidden")) {
                e.text("[ - ]");
                c.addClass("active");
                d.slideDown("fast");
                b.text("[ - ] Collapse all")
            } else {
                c.removeClass("active");
                d.slideUp("fast");
                b.text("[ + ] Expand all");
                e.text("[ + ]")
            }
            return false
        });
        a(".expand-link").click(function () {
            var b = a(this);
            var c = b.parents(".expand-group");
            var d = c.children(".expand-content");
            var e = b.find(".expand-toggle");
            var f = a(".expand-all");
            if (d.is(":hidden")) {
                e.text("[ - ]");
                c.addClass("active");
                d.slideDown("fast")
            } else {
                e.text("[ + ]");
                c.removeClass("active");
                d.slideUp("fast");
                f.text("[ + ] Expand all")
            }
            return false
        })
    })
})(jQuery);
(function () {
    $(document).ready(function () {
        var a = {
            $flowbar: $(".flowbar"), flowmessage: false, flowseconds: 0, flowtimer: "", init: function () {
                a.flowmessage = a.$flowbar.find("div.container").html();
                a.hasMessage = /<[^<]+?>/g.test(a.flowmessage);
                a.flowseconds = parseInt(a.$flowbar.data("flowseconds")) || 0;
                if (a.hasMessage) {
                    a.$flowbar.slideDown(500);
                    if (a.flowseconds) {
                        a.flowtimer = setTimeout(function () {
                            a.$flowbar.slideUp(500)
                        }, a.flowseconds * 1e3)
                    }
                }
            }
        };
        a.init()
    })
})();

/**
 *  Ajax Autocomplete for jQuery, version 1.2.9
 *  (c) 2013 Tomas Kirda
 *
 *  Ajax Autocomplete for jQuery is freely distributable under the terms of an MIT-style license.
 *  For details, see the web site: https://github.com/devbridge/jQuery-Autocomplete
 *
 */
(function (d) { "function" === typeof define && define.amd ? define(["jquery"], d) : d(jQuery) })(function (d) {
    function g(a, b) {
        var c = function () { }, c = {
            autoSelectFirst: !1, appendTo: "body", serviceUrl: null, lookup: null, onSelect: null, width: "auto", minChars: 1, maxHeight: 300, deferRequestBy: 0, params: {}, formatResult: g.formatResult, delimiter: null, zIndex: 9999, type: "GET", noCache: !1, onSearchStart: c, onSearchComplete: c, onSearchError: c, containerClass: "autocomplete-suggestions", tabDisabled: !1, dataType: "text", currentRequest: null, triggerSelectOnValidInput: !0,
            lookupFilter: function (a, b, c) { return -1 !== a.value.toLowerCase().indexOf(c) }, paramName: "query", transformResult: function (a) { return "string" === typeof a ? d.parseJSON(a) : a }
        }; this.element = a; this.el = d(a); this.suggestions = []; this.badQueries = []; this.selectedIndex = -1; this.currentValue = this.element.value; this.intervalId = 0; this.cachedResponse = {}; this.onChange = this.onChangeInterval = null; this.isLocal = !1; this.suggestionsContainer = null; this.options = d.extend({}, c, b); this.classes = { selected: "autocomplete-selected", suggestion: "autocomplete-suggestion" };
        this.hint = null; this.hintValue = ""; this.selection = null; this.initialize(); this.setOptions(b)
    } var k = function () { return { escapeRegExChars: function (a) { return a.replace(/[\-\[\]\/\{\}\(\)\*\+\?\.\\\^\$\|]/g, "\\$&") }, createNode: function (a) { var b = document.createElement("div"); b.className = a; b.style.position = "absolute"; b.style.display = "none"; return b } } }(); g.utils = k; d.Autocomplete = g; g.formatResult = function (a, b) { var c = "(" + k.escapeRegExChars(b) + ")"; return a.value.replace(RegExp(c, "gi"), "<strong>$1</strong>") }; g.prototype =
  {
      killerFn: null, initialize: function () {
          var a = this, b = "." + a.classes.suggestion, c = a.classes.selected, e = a.options, f; a.element.setAttribute("autocomplete", "off"); a.killerFn = function (b) { 0 === d(b.target).closest("." + a.options.containerClass).length && (a.killSuggestions(), a.disableKillerFn()) }; a.suggestionsContainer = g.utils.createNode(e.containerClass); f = d(a.suggestionsContainer); f.appendTo(e.appendTo); "auto" !== e.width && f.width(e.width); f.on("mouseover.autocomplete", b, function () { a.activate(d(this).data("index")) });
          f.on("mouseout.autocomplete", function () { a.selectedIndex = -1; f.children("." + c).removeClass(c) }); f.on("click.autocomplete", b, function () { a.select(d(this).data("index")) }); a.fixPosition(); a.fixPositionCapture = function () { a.visible && a.fixPosition() }; d(window).on("resize.autocomplete", a.fixPositionCapture); a.el.on("keydown.autocomplete", function (b) { a.onKeyPress(b) }); a.el.on("keyup.autocomplete", function (b) { a.onKeyUp(b) }); a.el.on("blur.autocomplete", function () { a.onBlur() }); a.el.on("focus.autocomplete", function () { a.onFocus() });
          a.el.on("change.autocomplete", function (b) { a.onKeyUp(b) })
      }, onFocus: function () { this.fixPosition(); if (this.options.minChars <= this.el.val().length) this.onValueChange() }, onBlur: function () { this.enableKillerFn() }, setOptions: function (a) { var b = this.options; d.extend(b, a); if (this.isLocal = d.isArray(b.lookup)) b.lookup = this.verifySuggestionsFormat(b.lookup); d(this.suggestionsContainer).css({ "max-height": b.maxHeight + "px", width: b.width + "px", "z-index": b.zIndex }) }, clearCache: function () {
          this.cachedResponse = {}; this.badQueries =
          []
      }, clear: function () { this.clearCache(); this.currentValue = ""; this.suggestions = [] }, disable: function () { this.disabled = !0; this.currentRequest && this.currentRequest.abort() }, enable: function () { this.disabled = !1 }, fixPosition: function () { var a; "body" === this.options.appendTo && (a = this.el.offset(), a = { top: a.top + this.el.outerHeight() + "px", left: a.left + "px" }, "auto" === this.options.width && (a.width = this.el.outerWidth() - 2 + "px"), d(this.suggestionsContainer).css(a)) }, enableKillerFn: function () {
          d(document).on("click.autocomplete",
          this.killerFn)
      }, disableKillerFn: function () { d(document).off("click.autocomplete", this.killerFn) }, killSuggestions: function () { var a = this; a.stopKillSuggestions(); a.intervalId = window.setInterval(function () { a.hide(); a.stopKillSuggestions() }, 50) }, stopKillSuggestions: function () { window.clearInterval(this.intervalId) }, isCursorAtEnd: function () {
          var a = this.el.val().length, b = this.element.selectionStart; return "number" === typeof b ? b === a : document.selection ? (b = document.selection.createRange(), b.moveStart("character",
          -a), a === b.text.length) : !0
      }, onKeyPress: function (a) {
          if (!this.disabled && !this.visible && 40 === a.which && this.currentValue) this.suggest(); else if (!this.disabled && this.visible) {
              switch (a.which) {
                  case 27: this.el.val(this.currentValue); this.hide(); break; case 39: if (this.hint && this.options.onHint && this.isCursorAtEnd()) { this.selectHint(); break } return; case 9: if (this.hint && this.options.onHint) { this.selectHint(); return } case 13: if (-1 === this.selectedIndex) { this.hide(); return } this.select(this.selectedIndex); if (9 === a.which &&
                  !1 === this.options.tabDisabled) return; break; case 38: this.moveUp(); break; case 40: this.moveDown(); break; default: return
              } a.stopImmediatePropagation(); a.preventDefault()
          }
      }, onKeyUp: function (a) { var b = this; if (!b.disabled) { switch (a.which) { case 38: case 40: return } clearInterval(b.onChangeInterval); if (b.currentValue !== b.el.val()) if (b.findBestHint(), 0 < b.options.deferRequestBy) b.onChangeInterval = setInterval(function () { b.onValueChange() }, b.options.deferRequestBy); else b.onValueChange() } }, onValueChange: function () {
          var a =
          this.options, b = this.el.val(), c = this.getQuery(b); this.selection && (this.selection = null, (a.onInvalidateSelection || d.noop).call(this.element)); clearInterval(this.onChangeInterval); this.currentValue = b; this.selectedIndex = -1; if (a.triggerSelectOnValidInput && (b = this.findSuggestionIndex(c), -1 !== b)) { this.select(b); return } c.length < a.minChars ? this.hide() : this.getSuggestions(c)
      }, findSuggestionIndex: function (a) {
          var b = -1, c = a.toLowerCase(); d.each(this.suggestions, function (a, d) {
              if (d.value.toLowerCase() === c) return b =
              a, !1
          }); return b
      }, getQuery: function (a) { var b = this.options.delimiter; if (!b) return a; a = a.split(b); return d.trim(a[a.length - 1]) }, getSuggestionsLocal: function (a) { var b = this.options, c = a.toLowerCase(), e = b.lookupFilter, f = parseInt(b.lookupLimit, 10), b = { suggestions: d.grep(b.lookup, function (b) { return e(b, a, c) }) }; f && b.suggestions.length > f && (b.suggestions = b.suggestions.slice(0, f)); return b }, getSuggestions: function (a) {
          var b, c = this, e = c.options, f = e.serviceUrl, l, g; e.params[e.paramName] = a; l = e.ignoreParams ? null : e.params;
          c.isLocal ? b = c.getSuggestionsLocal(a) : (d.isFunction(f) && (f = f.call(c.element, a)), g = f + "?" + d.param(l || {}), b = c.cachedResponse[g]); b && d.isArray(b.suggestions) ? (c.suggestions = b.suggestions, c.suggest()) : c.isBadQuery(a) || !1 === e.onSearchStart.call(c.element, e.params) || (c.currentRequest && c.currentRequest.abort(), c.currentRequest = d.ajax({ url: f, data: l, type: e.type, dataType: e.dataType }).done(function (b) { c.currentRequest = null; c.processResponse(b, a, g); e.onSearchComplete.call(c.element, a) }).fail(function (b, d, f) {
              e.onSearchError.call(c.element,
                a, b, d, f)
          }))
      }, isBadQuery: function (a) { for (var b = this.badQueries, c = b.length; c--;) if (0 === a.indexOf(b[c])) return !0; return !1 }, hide: function () { this.visible = !1; this.selectedIndex = -1; d(this.suggestionsContainer).hide(); this.signalHint(null) }, suggest: function () {
          if (0 === this.suggestions.length) this.hide(); else {
              var a = this.options, b = a.formatResult, c = this.getQuery(this.currentValue), e = this.classes.suggestion, f = this.classes.selected, g = d(this.suggestionsContainer), k = a.beforeRender, m = "", h; if (a.triggerSelectOnValidInput &&
            (h = this.findSuggestionIndex(c), -1 !== h)) { this.select(h); return } d.each(this.suggestions, function (a, d) { m += '<div class="' + e + '" data-index="' + a + '">' + b(d, c) + "</div>" }); "auto" === a.width && (h = this.el.outerWidth() - 2, g.width(0 < h ? h : 300)); g.html(m); a.autoSelectFirst && (this.selectedIndex = 0, g.children().first().addClass(f)); d.isFunction(k) && k.call(this.element, g); g.show(); this.visible = !0; this.findBestHint()
          }
      }, findBestHint: function () {
          var a = this.el.val().toLowerCase(), b = null; a && (d.each(this.suggestions, function (c,
                                                                                        d) { var f = 0 === d.value.toLowerCase().indexOf(a); f && (b = d); return !f }), this.signalHint(b))
      }, signalHint: function (a) { var b = ""; a && (b = this.currentValue + a.value.substr(this.currentValue.length)); this.hintValue !== b && (this.hintValue = b, this.hint = a, (this.options.onHint || d.noop)(b)) }, verifySuggestionsFormat: function (a) { return a.length && "string" === typeof a[0] ? d.map(a, function (a) { return { value: a, data: null } }) : a }, processResponse: function (a, b, c) {
          var d = this.options; a = d.transformResult(a, b); a.suggestions = this.verifySuggestionsFormat(a.suggestions);
          d.noCache || (this.cachedResponse[c] = a, 0 === a.suggestions.length && this.badQueries.push(c)); b === this.getQuery(this.currentValue) && (this.suggestions = a.suggestions, this.suggest())
      }, activate: function (a) { var b = this.classes.selected, c = d(this.suggestionsContainer), e = c.children(); c.children("." + b).removeClass(b); this.selectedIndex = a; return -1 !== this.selectedIndex && e.length > this.selectedIndex ? (a = e.get(this.selectedIndex), d(a).addClass(b), a) : null }, selectHint: function () {
          var a = d.inArray(this.hint, this.suggestions);
          this.select(a)
      }, select: function (a) { this.hide(); this.onSelect(a) }, moveUp: function () { -1 !== this.selectedIndex && (0 === this.selectedIndex ? (d(this.suggestionsContainer).children().first().removeClass(this.classes.selected), this.selectedIndex = -1, this.el.val(this.currentValue), this.findBestHint()) : this.adjustScroll(this.selectedIndex - 1)) }, moveDown: function () { this.selectedIndex !== this.suggestions.length - 1 && this.adjustScroll(this.selectedIndex + 1) }, adjustScroll: function (a) {
          var b = this.activate(a), c, e; b && (b = b.offsetTop,
          c = d(this.suggestionsContainer).scrollTop(), e = c + this.options.maxHeight - 25, b < c ? d(this.suggestionsContainer).scrollTop(b) : b > e && d(this.suggestionsContainer).scrollTop(b - this.options.maxHeight + 25), this.el.val(this.getValue(this.suggestions[a].value)), this.signalHint(null))
      }, onSelect: function (a) {
          var b = this.options.onSelect; a = this.suggestions[a]; this.currentValue = this.getValue(a.value); this.el.val(this.currentValue); this.signalHint(null); this.suggestions = []; this.selection = a; d.isFunction(b) && b.call(this.element,
          a)
      }, getValue: function (a) { var b = this.options.delimiter, c; if (!b) return a; c = this.currentValue; b = c.split(b); return 1 === b.length ? a : c.substr(0, c.length - b[b.length - 1].length) + a }, dispose: function () { this.el.off(".autocomplete").removeData("autocomplete"); this.disableKillerFn(); d(window).off("resize.autocomplete", this.fixPositionCapture); d(this.suggestionsContainer).remove() }
  }; d.fn.autocomplete = function (a, b) {
      return 0 === arguments.length ? this.first().data("autocomplete") : this.each(function () {
          var c = d(this), e =
          c.data("autocomplete"); if ("string" === typeof a) { if (e && "function" === typeof e[a]) e[a](b) } else e && e.dispose && e.dispose(), e = new g(this, a), c.data("autocomplete", e)
      })
  }
});

(function () {
    $(document).ready(function () {

        $('#shop-zipcode').autocomplete({
            paramName: 'zipCode',
            lookupLimit: 3,
            minChars: 2,
            serviceUrl: '/Umbraco/Surface/ComparePlansSurface/GetZipCodeList',
            onSelect: function (suggestion) {
                $('#shop-county').text(suggestion.data).show();
            },
            transformResult: function (response) {
                response = JSON.parse(response);
                return {
                    suggestions: $.map(response, function (dataItem) {
                        return { value: dataItem.zipCode, data: dataItem.county };
                    })
                };
            }
        });

        var initVisibilityFunc = function (id) {
            return function () {
                if (this.checked) {
                    $(id).show();
                } else {
                    $(id).hide();
                }
            };
        }

        $('#shop-coverself-check').click(function () {
            $("#shop-coverself").toggle(this.checked);
        }).each(initVisibilityFunc("#shop-coverself"));

        $('#shop-coverpartner-check').click(function () {
            $("#shop-coverpartner").toggle(this.checked);
        }).each(initVisibilityFunc("#shop-coverpartner"));

        var childTemplate = $('#shop-coverchild .template').clone().removeClass('template'),
          childAmmount = 1;
        $('#shop-coverchildren-check').click(function () {
            $("#shop-coverchild").toggle(this.checked);
            if (this.checked) {
                if ($('#shop-coverchild').children().length == 1)
                    addChild();
            }

            function addChild() {
                var template = childTemplate.clone();
                template.find('input').attr('name', 'ChildrenAges');
                template.find('.age').text(childAmmount);
                template.find('.add-child').click(function (e) {
                    e.preventDefault();
                    addChild();
                });
                template.find('.remove-child').click(function (e) {
                    e.preventDefault();
                    removeChild(this);
                });
                template.appendTo('#shop-coverchild');
                childAmmount++;
            }

            function removeChild(element) {
                $(element).parents('.child').remove();
                if ($('#shop-coverchild').children().length == 1) {
                    $("#shop-coverchild").toggle(false);
                    $('#shop-coverchildren-check').prop('checked', false);
                }
                childAmmount--;

                $.each($('#shop-coverchild .child'), function (index, child) {
                    $(child).find('.age').text(index);
                });
            }
        }).each(initVisibilityFunc("#shop-coverchild"));

        $('.hri-registration input[name=user_type]').change(function () {
            var type = $(this).val();
            if (type == 'register')
                $('#register-form').show();
            else
                $('#register-form').hide();
            $('.hri-registration .selection .desc').hide();
            $(this).next().next().css('display', 'inline-block');
        });
    })

    // Disable buttons after click
    $('form').submit(function (e) {
        if ($(this).valid && $(this).valid() || !$(this).valid)
            $(this).find('.disable-after-click').prop('disabled', true).addClass('disabled');
    });
})();