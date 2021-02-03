using DrawTest.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DrawTest.Class
{
    public class DrawLots
    {
        public List<Player> Players { get; private set; }

        public DrawLots(List<Player> players)
        {
            Players = players;
        }

        public List<DrawPosition> Draw()
        {
            var lotsCount = GetLotCount(Players.Count, out int depth);
            var startPos = GetStartPos(depth);
            var drawPositions = new List<DrawPosition>(lotsCount);
            var seedCount = Players.Count(p => p.Seed.HasValue);
            var byeCount = lotsCount - Players.Count;
            var nodeList = new List<Node<DrawPositionNode>>();
            var seedDrawpositions = new List<DrawPosition>(seedCount);
            for (int i = 0; i < lotsCount; i++)
            {
                var drawPosition = new DrawPosition()
                {
                    SortOrder = i + 1,
                    LotNumber = (int)startPos[i]
                };
                if (drawPosition.LotNumber <= seedCount)
                {
                    var seedPlayer = Players.FirstOrDefault(
                        p => p.Seed == drawPosition.LotNumber);
                    if (seedPlayer != null)
                    {
                        drawPosition.RegisterId = seedPlayer.Id;
                        drawPosition.PlayerName = seedPlayer.Name;
                        drawPosition.Delegation = seedPlayer.DelegationName;
                        drawPosition.Seed = seedPlayer.Seed;
                        seedDrawpositions.Add(drawPosition);
                    }
                }
                if (drawPosition.LotNumber > lotsCount - byeCount)
                {
                    drawPosition.PlayerName = "Bye";
                }
                drawPositions.Add(drawPosition);
                nodeList.Add(new Node<DrawPositionNode>
                {
                    Data = new DrawPositionNode
                    {
                        DrawPosition = drawPosition,
                        DelegationName = drawPosition?.Delegation,
                        FreeCount = drawPosition.PlayerName == null ? 1 : 0
                    }
                });
            }

            var players = Players.Where(p => p.Seed == null).ToList();
            var tempNodeList = new List<Node<DrawPositionNode>>();

            for (int i = lotsCount >> 1; i > 0; i >>= 1)
            {
                tempNodeList.Clear();
                tempNodeList.AddRange(nodeList);
                nodeList.Clear();
                Node<DrawPositionNode> node = null;
                for (int j = 0; j < tempNodeList.Count; j++)
                {
                    if (j % 2 == 0)
                    {
                        node = new Node<DrawPositionNode>
                        {
                            LeftChild = tempNodeList[j],
                            Data = new DrawPositionNode
                            {
                                FreeCount = tempNodeList[j].Data.FreeCount,
                                DelegationName = tempNodeList[j].Data.DelegationName
                            }
                        };
                        nodeList.Add(node);
                    }
                    else
                    {
                        node.RightChild = tempNodeList[j];
                        node.Data.FreeCount += tempNodeList[j].Data.FreeCount;
                        if (node.Data.DelegationName !=
                            tempNodeList[j].Data.DelegationName)
                        {
                            if (node.Data.DelegationName != null)
                            {
                                if (tempNodeList[j].Data.DelegationName != null)
                                {
                                    node.Data.DelegationName = null;
                                }
                            }
                            else
                            {
                                node.Data.DelegationName = tempNodeList[j].Data.DelegationName;
                            }
                        }
                    }
                }
            }

            var provider = new DrawProvider<Player>();
            provider.Shuffle(players);
            var groups = players.GroupBy(p => p.DelegationName).ToList();

            foreach (var group in groups)
            {
                players = group.ToList();
                var tempList = new List<DrawPositionNodeWithData>();
                var nodes = new List<DrawPositionNodeWithData>
                {
                    new DrawPositionNodeWithData
                    {
                        Players = players,
                        Node = nodeList[0]
                    }
                };
                for (int i = 0; i <= depth; i++)
                {
                    tempList.Clear();
                    tempList.AddRange(nodes);
                    nodes.Clear();
                    foreach (var item in tempList)
                    {
                        item.Node.Data.FreeCount -= item.Players.Count;
                        var position = item.Node.Data.DrawPosition;
                        if (item.Node.Data.DrawPosition != null)
                        {
                            position.RegisterId = item.Players[0].Id;
                            position.PlayerName = item.Players[0].Name;
                            position.Delegation = item.Players[0].DelegationName;
                        }
                        else
                        {
                            var node = item.Node;
                            if (item.Players.Count == 1)
                            {
                                if (node.LeftChild.Data.FreeCount >=
                                    node.RightChild.Data.FreeCount)
                                {
                                    nodes.Add(new DrawPositionNodeWithData
                                    {
                                        Node = node.LeftChild,
                                        Players = item.Players
                                    });
                                }
                                else
                                {
                                    nodes.Add(new DrawPositionNodeWithData
                                    {
                                        Node = node.RightChild,
                                        Players = item.Players
                                    });
                                }
                            }
                            else
                            {
                                Node<DrawPositionNode> moreFreeCountNode, lessFreeCountNode;

                                if (node.LeftChild.Data.FreeCount >=
                                    node.RightChild.Data.FreeCount)
                                {
                                    moreFreeCountNode = node.LeftChild;
                                    lessFreeCountNode = node.RightChild;
                                }
                                else
                                {
                                    moreFreeCountNode = node.RightChild;
                                    lessFreeCountNode = node.LeftChild;
                                }

                                var playersCount = item.Players.Count;
                                var lessPlayerCount = playersCount / 2;

                                if (lessPlayerCount > lessFreeCountNode.Data.FreeCount)
                                {
                                    lessPlayerCount = lessFreeCountNode.Data.FreeCount;
                                }

                                if (lessPlayerCount != 0)
                                    nodes.Add(new DrawPositionNodeWithData
                                    {
                                        Node = lessFreeCountNode,
                                        Players = item.Players.
                                            Take(lessPlayerCount).ToList()
                                    });

                                nodes.Add(new DrawPositionNodeWithData
                                {
                                    Node = moreFreeCountNode,
                                    Players = item.Players.
                                        TakeLast(playersCount - lessPlayerCount).ToList()
                                });
                            }
                        }
                    }
                }
            }
            foreach (var seedDrawPosition in seedDrawpositions)
            {
                DrawPosition anotherDrawPosition;
                var seedSortOrder = seedDrawPosition.SortOrder;
                if (seedSortOrder % 2 == 0)
                {
                    anotherDrawPosition = drawPositions[seedSortOrder - 2];
                }
                else
                {
                    anotherDrawPosition = drawPositions[seedSortOrder];
                }
                if (anotherDrawPosition.Seed != null)
                    continue;
                if (seedDrawPosition.Delegation != anotherDrawPosition.Delegation)
                    continue;
                var isDiffrentDelegation = false;
                foreach (var position in drawPositions)
                {
                    if (position.SortOrder % 2 == 1)
                    {
                        isDiffrentDelegation = position.Delegation !=
                            seedDrawPosition.Delegation;
                    }
                    else if (isDiffrentDelegation)
                    {
                        if (position.Delegation != seedDrawPosition.Delegation)
                        {
                            var tempPosition = position;
                            if (tempPosition.Seed != null)
                            {
                                tempPosition = drawPositions[tempPosition.SortOrder - 2];
                                if (tempPosition.Seed != null)
                                {
                                    isDiffrentDelegation = false;
                                    continue;
                                }
                            }
                            var tempDelegation = tempPosition.Delegation;
                            var tempPlayerName = tempPosition.PlayerName;
                            var tempRegisterId = tempPosition.RegisterId;
                            tempPosition.Delegation = anotherDrawPosition.Delegation;
                            tempPosition.PlayerName = anotherDrawPosition.PlayerName;
                            tempPosition.RegisterId = anotherDrawPosition.RegisterId;
                            anotherDrawPosition.RegisterId = tempRegisterId;
                            anotherDrawPosition.PlayerName = tempPlayerName;
                            anotherDrawPosition.Delegation = tempDelegation;
                            break;
                        }
                    }
                }
            }
            return drawPositions;
        }

        private int GetLotCount(
            int registerCount,
            out int depth)
        {
            depth = 0;
            if (registerCount == 0)
                return 0;
            var result = 1;

            while (registerCount > result)
            {
                depth++;
                result <<= 1;
            }
            return result;
        }

        private ArrayList GetStartPos(int depth)
        {
            var result = new ArrayList();

            if (depth < 1 || depth > 9)
            {
                throw new Exception("请确认人数在2到256之间。");
            }

            ArrayList aryPosTemp = new ArrayList();

            for (int nCyc = 0; nCyc < depth; nCyc++)
            {
                //第一次开始时,手动赋值
                if (nCyc == 0)
                {
                    if (aryPosTemp.Count == 0)
                    {
                        aryPosTemp.Add(0);	//从0开始计数
                        aryPosTemp.Add(1);
                    }
                }
                else
                {
                    int nCntHalf = aryPosTemp.Count; //新组的半区空间等于上一组总空间
                    ArrayList aryPosNew = new ArrayList(); //临时用的新区
                    int byValue;

                    //填入新组上半区内容
                    for (int nCycNew = 0; nCycNew < nCntHalf; nCycNew++)
                    {
                        byValue = (int)aryPosTemp[nCycNew];

                        if (nCycNew % 2 == 0) //奇数位置和偶数位置
                            aryPosNew.Add((byValue + 1) * 2 - 1 - 1);
                        else
                            aryPosNew.Add((byValue + 1) * 2 - 1);
                    }

                    //填入下半区内容
                    for (int nCycNew = 0; nCycNew < nCntHalf; nCycNew++)
                    {
                        //是根据上半区相对位置值决定的
                        byValue = (int)aryPosNew[nCycNew];

                        if (nCycNew % 2 == 0)
                            aryPosNew.Add(byValue + 2);
                        else
                            aryPosNew.Add(byValue - 2);
                    }

                    //把新区内容付给临时区
                    aryPosTemp.Clear();
                    for (int nCycNew = 0; nCycNew < aryPosNew.Count; nCycNew++)
                    {
                        byValue = (int)aryPosNew[nCycNew];
                        aryPosTemp.Add(byValue);
                    }
                }
            }
            //把临时数组输出
            result.Clear();
            for (int nCycNew = 0; nCycNew < aryPosTemp.Count; nCycNew++)
            {
                result.Add((int)aryPosTemp[nCycNew] + 1);
            }
            return result;
        }
    }
}
