fn isConvex obj =
(
	local convex = true
	
	-- set the selection level to vertex
	subObjectLevel = 1
	
	-- selects the concave vertex
	PolyToolsSelect.ConvexConcave 0.0 1
	
	if (obj.selectedVerts.count > 0) then
		convex = false
	
	-- back to object selection level
	subobjectLevel = 0
	
	return convex
)

convexity = isConvex $
print("convex: " + convexity as string)