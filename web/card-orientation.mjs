// A rectangle rotated 180° occupies the same space and has the same cover graph.
export function playerFacingAngle(angle){const a=((Number(angle)%360)+540)%360-180;return a>90?a-180:a< -90?a+180:a;}
