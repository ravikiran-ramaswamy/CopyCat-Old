class classes.CAmmoManager
{
	var ammos:Array;
		
	public function CAmmoManager(){
		trace("Initialize ammomanager.");
		this.ammos = new Array();
	}
	function Add(x:Number, y:Number, s:Number, c:String)
	{
		this.ammos[this.ammos.length] = (new classes.CAmmo(x, y, s, c,this.ammos.length));
	}
		
	public function clear(){
		trace("Clearing ammos.");
		for(var i=0; i<this.ammos.length;i++){
			this.ammos[i].Remove(); 
			delete this.ammos[i];
		}
	}
		
	public function DoFrame(){
		for(var i=0; i<this.ammos.length;i++)
		{
			this.ammos[i].Move();	
			
			_root.x_cord = this.ammos[i].Clip._x
			_root.y_cord = this.ammos[i].Clip._y
			
			
			if (this.ammos[i].Clip._x > 800 || this.ammos[i].Clip._x < 0){
				this.ammos[i].Remove();
				delete this.ammos[i];
			}
			/*
			for(var j=0;j<_root.trans_game.game.ennemymanager.ennemies.length;j++){				
				trace(_root.trans_game.game.ennemymanager.ennemies.length) ;
				if(_root.trans_game.game.ennemymanager.ennemies[j].Clip.hitTest(this.ammos[i].Clip)){
					_root.gameScore += 5;
					_root.trans_game.game.ennemymanager.ennemies[j].Explose();
					_root.trans_game.game.ennemymanager.ennemies[j] = new classes.CEnnemy(random(600), 7, "enemy");
					this.ammos[i].Remove(); 
					delete this.ammos[i];
				}
			}*/		
			for (var j = 0; j<2; j++)
			{
				_root.jlabel = j;
			}
			for(var enem_num=0; enem_num <_root.trans_game.game.ennemymanager.ennemies.length;enem_num++){				
				_root.enemyL = _root.trans_game.game.ennemymanager.ennemies.length;
				trace("ammos"+i+":  "+this.ammos[i].Clip._x +"   "+this.ammos[i].Clip._y )
				trace("enemies"+enem_num+":"+_root.trans_game.game.ennemymanager.ennemies[enem_num].Clip._x + "  " +_root.trans_game.game.ennemymanager.ennemies[enem_num].Clip._y);
				_root.ilabel = i;
				
				_root.e_x_cord = _root.trans_game.game.ennemymanager.ennemies[enem_num].Clip._x
				_root.e_y_cord = _root.trans_game.game.ennemymanager.ennemies[enem_num].Clip._y
				
				if(this.ammos[i].Clip.hitTest(_root.trans_game.game.ennemymanager.ennemies[enem_num].Clip)){
					_root.gameScore += 23;
					_root.trans_game.game.ennemymanager.ennemies[enem_num].Explose();
					_root.trans_game.game.ennemymanager.ennemies[enem_num] = new classes.CEnnemy(random(600), 1, "enemy");
					this.ammos[i].Remove(); 
					delete this.ammos[i];
				}
			}
		}
	}
}