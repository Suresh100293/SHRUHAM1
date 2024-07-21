/*!
 * jquery.fancytree.ariagrid.js
 *
 * Support ARIA compliant markup and keyboard navigation for tree grids with
 * embedded input controls.
 * (Extension module for jquery.fancytree.js: https://github.com/mar10/fancytree/)
 *
 * Note: this extension also requires `ext-table` to be active.
 *
 * Copyright (c) 2008-2017, Martin Wendt (http://wwWendt.de)
 *
 * Released under the MIT license
 * https://github.com/mar10/fancytree/wiki/LicenseInfo
 *
 * @version @VERSION
 * @date @DATE
 */

;(function($, window, document, undefined) {

"use strict";

/*
  - References:
    - https://github.com/w3c/aria-practices/issues/132
    - https://rawgit.com/w3c/aria-practices/treegrid/examples/treegrid/treegrid-row-nav-primary-1.html
    - https://github.com/mar10/fancytree/issues/709

  - TODO: enable treeOpts.aria by default
    (requires some benchmarks, confirm it does not affect performance too much)
  - TODO: make ext-ariagrid part of ext-table (enable behavior with treeOpts.aria option)
    (Requires stable specification)
*/


/*******************************************************************************
 * Private functions and variables
 */

// Allow these navigation keys even when input controls are focused

var	clsFancytreeActiveCell = "fancytree-active-cell",
	// TODO: define attribute- and class-names for better compression
	// Define which keys are handled by embedded control, and should *not* be 
	// passed to tree navigation handler:
	NAV_KEYS = {
		"text": ["left", "right", "home", "end", "backspace"],
		"number": ["up", "down", "left", "right", "home", "end", "backspace"],
		"checkbox": [],
		"link": [],
		"radiobutton": ["up", "down"],
		"select-one": ["up", "down"],
		"select-multiple": ["up", "down"]
	};


/* Calculate TD column index (considering colspans).*/
function getColIdx($tr, $td) {
	var colspan,
		td = $td.get(0),
		idx = 0;

	$tr.children().each(function () {
		if( this === td ) {
			return false;
		}
		colspan = $(this).prop("colspan");
		idx += colspan ? colspan : 1;
	});
	return idx;
}


/* Find TD at given column index (considering colspans).*/
function findTdAtColIdx($tr, colIdx) {
	var colspan,
		res = null,
		idx = 0;

	$tr.children().each(function () {
		if( idx >= colIdx ) {
			res = $(this);
			return false;
		}
		colspan = $(this).prop("colspan");
		idx += colspan ? colspan : 1;
	});
	return res;
}


/* Find adjacent cell for a given direction. Skip empty cells and consider merged cells */
function findNeighbourTd(tree, $target, keyCode){
	var $td = $target.closest("td"),
		$tr = $td.parent(),
		treeOpts = tree.options,
		colIdx = getColIdx($tr, $td),
		$tdNext = null;

	switch( keyCode ){
		case "left":
			$tdNext = treeOpts.rtl ? $td.next() : $td.prev();
			break;
		case "right":
			$tdNext = treeOpts.rtl ? $td.prev() : $td.next();
			break;
		case "up":
		case "down":
			while( true ) {
				$tr = keyCode === "up" ? $tr.prev() : $tr.next();
				if( !$tr.length ) {
					break;
				}
				// Skip hidden rows
				if( $tr.is(":hidden") ) {
					continue;
				}
				// Find adjacent cell in the same column
				$tdNext = findTdAtColIdx($tr, colIdx);
				// Skip cells that don't contain a focusable element
//				if( $tdNext && $tdNext.find(":input,a").length ) {
					break;
//				}
			}
			break;
		case "ctrl+home":
			$tdNext = findTdAtColIdx($tr.siblings().first(), colIdx);
			if( $tdNext.is(":hidden") ) {
				$tdNext = findNeighbourTd(tree, $tdNext.parent(), "down");
			}
			break;
		case "ctrl+end":
			$tdNext = findTdAtColIdx($tr.siblings().last(), colIdx);
			if( $tdNext.is(":hidden") ) {
				$tdNext = findNeighbourTd(tree, $tdNext.parent(), "up");
			}
			break;
		case "home":
			// TODO: rtl
			$tdNext = $tr.children("td").first();
			break;
		case "end":
			// TODO: rtl
			$tdNext = $tr.children("td").last();
			break;
	}
	return $tdNext;
}

/**
 * [ext-ariagrid] Set active cell and activate cell-mode if needed. 
 * Pass $td=null to enter row-mode.
 *
 * See also FancytreeNode#setActive(flag, {cell: idx})
 *
 * @param {jQuery | Element | integer} [$td] 
 * @alias Fancytree#activateCell
 * @requires jquery.fancytree.ariagrid.js
 * @since 2.23
*/
$.ui.fancytree._FancytreeClass.prototype.activateCell = function($td) {
	var $input, $tr,
		$prevTd = this.$activeTd || null,
		$prevTr = $prevTd ? $prevTd.closest("tr") : null;

	this.debug("activateCell: " + ($prevTd ? $prevTd.text() : "null") +
		" -> " + ($td ? $td.text() : "null"));

	if( $td && $td.length ) {
		this.$container.addClass("fancytree-cell-mode");
		$tr = $td.closest("tr");
		if( $prevTd ) {
			// cell-mode => cell-mode
			if( $prevTd.is($td) ) {
				return;
			}
			$prevTd
				// .removeAttr("tabindex")
				.removeClass(clsFancytreeActiveCell);
			if( !$prevTr.is($tr) ) {
				$.ui.fancytree.getNode($td).setActive();
			}
			// tree.$activeTd.find(":focus").blur();
		} else {

		}
		$td.addClass(clsFancytreeActiveCell);
		this.$activeTd = $td;

		$input = $td.find(":input:enabled,a");
		if( $input.length ) {
			$input.focus();
		} else {
			// $td.attr("tabindex", "0").focus();
			// tree.$container.focus();
		}
	} else {
		// $td == null: switch back to row-mode
		this.$container.removeClass("fancytree-cell-mode");
		if( $prevTd ) {
			// cell-mode => row-mode
			$prevTd
				.removeAttr("tabindex")
				.removeClass(clsFancytreeActiveCell);
			// we need to blur first, because otherwies the focus frame is not reliably removed(?)
			$prevTr.find("td").blur().removeAttr("tabindex");
			$prevTr.find(":input:enabled,a").attr("tabindex", "-1");
			this.$activeTd = null;
			// The cell lost focus, but the tree still needs to capture keys:
			this.setFocus();
		} else {
			// row-mode => row-mode (nothing to do)
		}
	}
};


/*******************************************************************************
 * Extension code
 */
$.ui.fancytree.registerExtension({
	name: "ariagrid",
	version: "@VERSION",
	// Default options for this extension.
	options: {
		cellFocus: "allow",
		// TODO: allow custom behavior?
		// enterMode: "default",
		// escapaMode: "default",
		// TODO: use a global tree option `name` or `title` instead?:
		label: "Tree Grid"  // Added as `aria-label` attribute
	},

	treeInit: function(ctx){
		var tree = ctx.tree,
			treeOpts = ctx.options,
			opts = treeOpts.ariagrid;

		// ariagrid requires the table extension to be loaded before itself
		this._requireExtension("table", true, true);
		if( !treeOpts.aria ) { $.error("ext-ariagrid requires `aria: true`"); }

		this._superApply(arguments);

		this.$activeTd = null;

		this.$container
			.addClass("fancytree-ext-ariagrid")
			.attr("aria-label", "" + opts.label);
		this.$container.find("thead > tr > th").attr("role", "columnheader");

		this.nodeColumnIdx = treeOpts.table.nodeColumnIdx;
		this.checkboxColumnIdx = treeOpts.table.checkboxColumnIdx;
		if( this.checkboxColumnIdx == null ) {
			this.checkboxColumnIdx = this.nodeColumnIdx;
		}

		// Activate node if embedded input gets focus (due to a click)
		this.$container.on("focusin", function(event){
			var node = $.ui.fancytree.getNode(event.target),
 				$td = $(event.target).closest("td");

			tree.debug("focusin: " + (node ? node.title : "null") +
				", target: " + ($td ? $td.text() : null) +
				", node was active: " + (node && node.isActive()) +
				", last cell: " + (tree.$activeTd ? tree.$activeTd.text() : null));
			tree.debug("focusin: target", event.target);

 			// TODO: 
			// if( ctx.options.cellFocus === "force" || ctx.options.cellFocus === "start" ) {
			// 	var $td = $(this.getFirstChild().tr).find(">td:first");
			// 	_activateCell(this, $td);
			// }
			if( node && /*tree.$activeTd && */ $td.length ) {
				// If in cell-mode, activate the new cell
 				// _local._activateCell($td);
			}
 		// 	// this._activateCell($td);
			// if( node && !node.isActive() ){
			// 	// Call node.setActive(), but also pass the event
			// 	ctx2 = ctx.tree._makeHookContext(node, event);
			// 	ctx.tree._callHook("nodeSetActive", ctx2, true, {cell: $td});
			// }
		});
	},
	nodeClick: function(ctx) {
		var targetType = ctx.targetType,
			tree = ctx.tree,
			node = ctx.node,
			$td = $(event.target).closest("td");

		tree.debug("nodeClick: node: " + (node ? node.title : "null") +
			", targetType: " + targetType +
			", target: " + ($td.length ? $td.text() : null) +
			", node was active: " + (node && node.isActive()) +
			", last cell: " + (tree.$activeTd ? tree.$activeTd.text() : null));
		// TODO: 
		// if( ctx.options.cellFocus === "force" || ctx.options.cellFocus === "start" ) {
		// 	var $td = $(this.getFirstChild().tr).find(">td:first");
		// 	_activateCell(this, $td);
		// }
		if( tree.$activeTd ) {
			// If in cell-mode, activate new cell
			tree.activateCell($td);
			return false;
		}
	},
	nodeRenderStatus: function(ctx) {
		// Set classes for current status
		var res,
			node = ctx.node,
			$tr = $(node.tr);

		res = this._super(ctx);

		if( node.parent ) {
			// TODO: consider moving this code to core (if aria: true):
			$tr
				.attr("aria-level", node.getLevel())
				.attr("aria-setsize", node.parent.children.length)
				.attr("aria-posinset", node.getIndex() + 1);

			if( $tr.is(":hidden") ) {
				$tr.attr("aria-hidden", true);
			} else {
				$tr.removeAttr("aria-hidden");
			}
			// TODO: remove aria-labelledby (set by core) or even remove it from core?
			// TODO: move or mirror aria-expanded attribute from TR to node-span or its parent TD?
		}
		return res;
	},
	nodeSetActive: function(ctx, flag, callOpts) {
		var node = ctx.node,
			tree = ctx.tree,
			$tr = $(node.tr);
			//event = ctx.originalEvent || {},
			//triggeredByInput = $(event.target).is(":input");

		flag = (flag !== false);
		node.debug("nodeSetActive(" + flag + ")");

		if( flag && callOpts && callOpts.cell ) {
			// TODO: `cell` may be an col-index, <td>, or `$(td)`
			tree.activateCell(callOpts.cell);
			return;
		}
		this._superApply(arguments);

		// Only the active TR has tabindex "0", all others have "-1"
		// In cell mode
		//   - the active TD has tabindex "-1" (removed from all others)
		//   - embedded inputs and <a> tags have tabindex "-1" for active row, "0" otherwise
		// TODO: should we only consider input:enabled?

		if( flag ){
			$tr.attr("tabindex", "0");
			$tr.find(">td :input,a").attr("tabindex", "0");
			// $tr.find(">td:not(:has(:input,a))").attr("tabindex", "0");
		} else {
			// Only active row is tabbable: 
			// this._local._activateCell(null);
			// $tr.find("td").removeAttr("tabindex");
			$tr.attr("tabindex", "-1");
			$tr.find(">td :input,a").attr("tabindex", "-1");
		}
	},
	nodeKeydown: function(ctx) {
		var colIdx, handleKeys, inputType, $td,
			tree = ctx.tree,
			node = ctx.node,
			treeOpts = ctx.options,
			opts = treeOpts.ariagrid,
			event = ctx.originalEvent,
			eventString = $.ui.fancytree.eventToString(event),
			$target = $(event.target),
			$activeTd = this.$activeTd,
			$activeTr = $activeTd ? $activeTd.closest("tr") : null;

		if( $target.is(":input:enabled") ) {
			inputType = $target.prop("type");
		} else if( $target.is("a") ) {
			inputType = "link";
		}
		ctx.tree.debug("nodeKeydown(" + eventString + "), activeTd: '" + 
			($activeTd && $activeTd.text()) + "', inputType: " + inputType);

		if( inputType && eventString !== "esc" ){
			handleKeys = NAV_KEYS[inputType];
			if( handleKeys && $.inArray(eventString, handleKeys) >= 0 ){
				return;  // Let input control handle the key
			}
		}
		// if( ctx.options.cellFocus === "force" || ctx.options.cellFocus === "start" ) {
		// 	var $td = $(this.getFirstChild().tr).find(">td:first");
		// 	_activateCell(this, $td);
		// }

		switch( eventString ) {
		case "right":
			if( $activeTd ) {
				// Cell mode: move to neighbour
				$td = findNeighbourTd(tree, $activeTd, eventString);
				tree.activateCell($td);
			} else if ( !node.isExpanded() && node.hasChildren() !== false ) {
				// Row mode and current node can be expanded: 
				// default handling will expand.
				break;
			} else {
				// Row mode: switch to cell mode
				$td = $(node.tr).find(">td:first");
				tree.activateCell($td);
			}
			return;  // no default handling

		case "left":
		case "home":
		case "end":
		case "ctrl+home":
		case "ctrl+end":
		case "up":
		case "down":
			if( $activeTd ) {
				// Cell mode: move to neighbour
				$td = findNeighbourTd(tree, $activeTd, eventString);
				tree.activateCell($td);
				return false;
			}
			break;

		case "esc":
			// TODO: make this behavior configurable?
			if( $activeTd && opts.cellFocus !== "force" ) {
				// Switch back from cell-mode to row-mode
				tree.activateCell(null);
				return;
			}
			break;

		case "return":
			if( $activeTd ) {
				colIdx = getColIdx($activeTr, $activeTd);
				// Apply 'default action' for embedded cell control
				if( colIdx === this.nodeColumnIdx ) {
					node.toggleExpanded();
					return false;
				} else if( colIdx === this.checkboxColumnIdx ) {
					node.toggleSelected	();
					return false;
				} else if( $activeTd.find(":checkbox:enabled").length ) {
					$activeTd.find(":checkbox:enabled").click();
					return false;
				} else if( inputType === "link" ) {
					$activeTd.find("a")[0].click();
					return false;
				}
			} else {
				// TODO: make this behavior configurable?
				// Switch from row mode to cell mode
				$td = $(node.tr).find(">td:nth(" + this.nodeColumnIdx + ")");
				tree.activateCell($td);
				return;  // no default handling
			}
			break;
		case "space":
			if( $activeTd && getColIdx($(node.tr), $activeTd) !== this.checkboxColumnIdx ) {
				// make sure we don't select whole row in cell-mode, unless the
				// checkbox cell is active
				return;  // no default handling
			}
			break;
		// default:
		}
		return this._superApply(arguments);
	}
});
}(jQuery, window, document));
