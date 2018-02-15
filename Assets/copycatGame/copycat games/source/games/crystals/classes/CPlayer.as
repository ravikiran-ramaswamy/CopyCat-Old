class classes.CPlayer
{
	var fake_xmouse =0;
	var fake_ymouse =0;
	var Clip:MovieClip;	
	
	public function CPlayer(){
		this.Clip = _root.trans_game.background.attachMovie("player", "playerclip", _root.trans_game.background.getNextHighestDepth());
	}
	
	public function Move(x:Number, y:Number){
	  fake_xmouse =getProperty("locator", _x)-this.Clip._x;
	  fake_ymouse =getProperty("locator", _y)-this.Clip._y;
		this.Clip._x = x + fake_xmouse/20;
		this.Clip._y = y + fake_ymouse/20;
		this.Clip.swapDepths(_root.trans_game.background.getNextHighestDepth());		
	}
	
	public function Fire(){
		_root.trans_game.game.ammomanager.Add(this.Clip._x, this.Clip._y, 40, "ammo1");
	}
}