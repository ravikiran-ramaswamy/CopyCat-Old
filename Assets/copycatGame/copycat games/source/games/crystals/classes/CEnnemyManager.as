class classes.CEnnemyManager
{
		var ennemies:Array;
		var MaxEnnemies:Number;
		
		public function CEnnemyManager(){
			trace("Initialize ammomanager.");
			this.ennemies = new Array();
			this.MaxEnnemies = 7;
			
			for(var i=0; i<this.MaxEnnemies;i++)
			{
				this.ennemies[i] = new classes.CEnnemy(random(600), 7, "enemy");
			}
		}
		function Add(y:Number, s:Number, c:String)
		{
			this.ennemies[this.ennemies.length] = (new classes.CEnnemy(y, s, c));
		}
		
			public function DoFrame(){
			for(var i=0; i<this.ennemies.length;i++)
			{
				this.ennemies[i].Move();
				if (this.ennemies[i].Clip._x < 0)
				{
					this.ennemies[i].Clip.removeMovieClip()
					this.ennemies[i].Clip.unloadMovie()
					delete this.ennemies[i];
					this.ennemies[i] = new classes.CEnnemy(random(600), 7, "enemy");
				}				
				if(this.ennemies[i].Clip.hitTest(_root.trans_game.game.player.Clip)){
					if(_root.gameScore >= 5)	
						_root.gameScore -= 5;
					
					_root.trans_game.game.player.Clip._x -= 30;
					_root.trans_game.game.player.Clip._y += 30;
					
					this.ennemies[i].Explose();
					this.ennemies[i] = new classes.CEnnemy(random(600), 7, "enemy");
				}
				this.ennemies.sort();
			}
		}
}