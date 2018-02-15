class classes.CGame
{
	var stars;
	var player;
	var ammomanager;
	var ennemymanager;
	var points;
	var glastmove;

	public function CGame(){
		trace("initiating game engine...");		
		this.player = new classes.CPlayer();		
		this.points = 0;		
		this.glastmove = getTimer();		
		this.ammomanager = new classes.CAmmoManager();
		this.ennemymanager = new classes.CEnnemyManager();				
	}	
	
	public function DoFrame(){
			this.ammomanager.DoFrame();
			this.ennemymanager.DoFrame();
	}
	
	public function GetMouse(){
		if(_root.trans_game.background._xmouse > 0 && _root.trans_game.background._ymouse > 0)
			this.player.Move(_root.trans_game.background._xmouse,_root.trans_game.background._ymouse);
	}
	
	public function MouseDown(){
		this.player.Fire();
	}
}